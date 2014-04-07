﻿namespace Stumps.Http
{

    using System;
    using System.Globalization;
    using System.Net;
    using System.Threading;

    /// <summary>
    ///     A class that represents a basic HTTP server.
    /// </summary>
    internal sealed class HttpServer : IHttpServer
    {

        private readonly IHttpHandler _handler;
        private readonly int _port;
        private HttpListener _listener;
        private bool _started;
        private Thread _thread;

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Stumps.Http.HttpServer"/> class.
        /// </summary>
        /// <param name="port">The port the HTTP server is using to listen for traffic.</param>
        /// <param name="handler">The default <see cref="T:Stumps.Http.IHttpHandler"/> executed when receiving traffic.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="handler"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="port"/> exceeds the allowed TCP port range.</exception>
        public HttpServer(int port, IHttpHandler handler)
        {

            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("port");
            }

            _listener = new HttpListener();
            _port = port;
            _handler = handler;

        }

        /// <summary>
        /// Finalizes an instance of the <see cref="T:Stumps.Http.HttpServer"/> class.
        /// </summary>
        ~HttpServer()
        {
            Dispose();
        }

        /// <summary>
        ///     Occurs when the server processed an incomming HTTP request and returned the response to the client.
        /// </summary>
        public event EventHandler<StumpsContextEventArgs> RequestFinished;

        /// <summary>
        ///     Occurs after the server has finished processing the HTTP request, 
        ///     and has constructed a response, but before it returned to the client.
        /// </summary>
        public event EventHandler<StumpsContextEventArgs> RequestProcessed;

        /// <summary>
        ///     Occurs when the server receives an incomming HTTP request.
        /// </summary>
        public event EventHandler<StumpsContextEventArgs> RequestReceived;

        /// <summary>
        ///     Gets TCP port used by the instance to listen for HTTP requests.
        /// </summary>
        /// <value>
        ///     The port used to listen for HTTP requets.
        /// </value>
        public int Port
        {
            get { return _port; }
        }

        /// <summary>
        /// Gets a value indicating whether the instance is started.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the instance is started; otherwise, <c>false</c>.
        /// </value>
        public bool Started
        {
            get { return _started; }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {

            if (_listener.IsListening)
            {
                _listener.Stop();
            }

            var disposable = _listener as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }

            GC.SuppressFinalize(this);

        }

        /// <summary>
        ///     Starts the instance listening for HTTP requests.
        /// </summary>
        public void StartListening()
        {

            if (_started)
            {
                return;
            }

            _started = true;

            _listener = new HttpListener();

            var url = string.Format(CultureInfo.InvariantCulture, BaseResources.HttpServerPattern, _port);

            _listener.Prefixes.Add(url);
            _listener.Start();
            _listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;

            _thread = new Thread(WaitForConnections);
            _thread.Start();

        }

        /// <summary>
        ///     Stops the instance from listening for HTTP requests.
        /// </summary>
        public void StopListening()
        {
            try
            {
                _started = false;
                _listener.Stop();
                _thread.Join();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        /// <summary>
        ///     Processes the incoming HTTP request asynchronously.
        /// </summary>
        /// <param name="asyncResult">The asynchronous result.</param>
        private void ProcessAsyncRequest(IAsyncResult asyncResult)
        {

            if (_listener == null)
            {
                return;
            }

            try
            {
                // Gets the HTTP context for the request
                var context = _listener.EndGetContext(asyncResult);

                StumpsHttpContext stumpsContext = null;

                // Create a new StumpsHttpContext
                stumpsContext = new StumpsHttpContext(context);

                if (this.RequestReceived != null)
                {
                    this.RequestReceived(this, new StumpsContextEventArgs(stumpsContext));
                }

                // Process the request through the HTTP handler
                _handler.ProcessRequest(stumpsContext);

                if (this.RequestProcessed != null)
                {
                    this.RequestProcessed(this, new StumpsContextEventArgs(stumpsContext));
                }

                // End the request
                stumpsContext.EndResponse();

                if (this.RequestFinished != null)
                {
                    this.RequestFinished(this, new StumpsContextEventArgs(stumpsContext));
                }

            }
            catch (HttpListenerException)
            {
            }
            catch (InvalidOperationException)
            {
            }

        }

        /// <summary>
        ///     Wait for incoming HTTP connections.
        /// </summary>
        private void WaitForConnections()
        {

            while (_started && _listener.IsListening)
            {
                var result = _listener.BeginGetContext(ProcessAsyncRequest, null);
                result.AsyncWaitHandle.WaitOne();
            }

        }

    }

}