﻿namespace Stumps.Proxy
{

    using System;
    using System.Collections.Generic;
    using Stumps.Http;

    internal class HttpPipelineHandler : IHttpHandler
    {

        private readonly List<IHttpHandler> _handlers;

        public HttpPipelineHandler()
        {
            _handlers = new List<IHttpHandler>();
        }

        public int Count
        {
            get { return _handlers.Count; }
        }

        public IHttpHandler this[int index]
        {
            get { return _handlers[index]; }
        }

        public ProcessHandlerResult ProcessRequest(IStumpsHttpContext context)
        {

            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var result = ProcessHandlerResult.Continue;

            foreach (var handler in _handlers)
            {

                result = handler.ProcessRequest(context);

                if (result == ProcessHandlerResult.Terminate)
                {
                    break;
                }

            }

            return result;

        }

        public void Add(IHttpHandler handler)
        {
            _handlers.Add(handler);
        }

    }

}