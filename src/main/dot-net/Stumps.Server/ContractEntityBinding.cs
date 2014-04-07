namespace Stumps.Server
{

    using System.Collections.Generic;
    using Stumps.Server.Data;

    /// <summary>
    ///     A helper class that provides a translation between contracts and entities.
    /// </summary>
    internal static class ContractEntityBinding
    {

        /// <summary>
        ///     Creates a Stump contract from a Stump data entity.
        /// </summary>
        /// <param name="entity">The <see cref="T:Stumps.Server.Data.StumpEntity"/> used to create the contract.</param>
        /// <returns>
        ///     A <see cref="T:Stumps.Server.StumpContract"/> created from the specified <paramref name="entity"/>.
        /// </returns>
        public static StumpContract CreateContractFromEntity(StumpEntity entity)
        {

            var contract = new StumpContract
            {
                Request = new RecordedRequest(new HttpRequestEntityReader(entity.Request), ContentDecoderHandling.DecodeNotRequired),
                Response = new RecordedResponse(new HttpResponseEntityReader(entity.Response), ContentDecoderHandling.DecodeNotRequired),
                Rules = new RuleContractCollection(),
                StumpCategory = entity.StumpName,
                StumpId = entity.StumpId,
                StumpName = entity.StumpName
            };

            foreach (var ruleEntity in entity.Rules)
            {
                var rule = new RuleContract
                {
                    RuleName = ruleEntity.RuleName
                };

                foreach (var value in ruleEntity.Settings)
                {
                    var setting = new RuleSetting
                    {
                        Name = value.Name,
                        Value = value.Value
                    };
                    rule.AppendRuleSetting(setting);
                }

                contract.Rules.Add(rule);
            }

            return contract;

        }

        /// <summary>
        ///     Creates a Stump data entity from a Stump contract.
        /// </summary>
        /// <param name="contract">The <see cref="T:Stumps.Server.StumpContract"/> used to create the entity.</param>
        /// <returns>
        ///     A <see cref="T:Stumps.Server.Data.StumpEntity"/> created from the specified <paramref name="contract"/>.
        /// </returns>
        public static StumpEntity CreateEntityFromContract(StumpContract contract)
        {

            var request = new HttpRequestEntity
            {
                BodyFileName = string.Empty,
                Headers = CreateNameValuePairFromHeaders(contract.Request.Headers),
                HttpMethod = contract.Request.HttpMethod,
                LocalEndPoint = contract.Request.LocalEndPoint.ToString(),
                ProtocolVersion = contract.Request.ProtocolVersion,
                RawUrl = contract.Request.RawUrl,
                RemoteEndPoint = contract.Request.RemoteEndPoint.ToString()
            };

            var response = new HttpResponseEntity
            {
                BodyFileName = string.Empty,
                Headers = CreateNameValuePairFromHeaders(contract.Response.Headers),
                RedirectAddress = contract.Response.RedirectAddress,
                StatusCode = contract.Response.StatusCode,
                StatusDescription = contract.Response.StatusDescription
            };

            var entity = new StumpEntity
            {
                Request = request,
                Response = response,
                StumpCategory = contract.StumpCategory,
                StumpId = contract.StumpId,
                StumpName = contract.StumpName
            };

            entity.Rules = new List<RuleEntity>();

            foreach (var rule in contract.Rules)
            {
                var ruleEntity = new RuleEntity
                {
                    RuleName = rule.RuleName,
                    Settings = new List<NameValuePairEntity>()
                };

                var settings = rule.GetRuleSettings();
                foreach (var setting in settings)
                {
                    ruleEntity.Settings.Add(
                        new NameValuePairEntity
                        {
                            Name = setting.Name,
                            Value = setting.Value
                        });
                }

                entity.Rules.Add(ruleEntity);

            }

            return entity;

        }

        /// <summary>
        ///     Creates a list of <see cref="T:Stumps.Server.Data.NameValuePairEntity"/> objects a <see cref="T:Stumps.IHttpHeaders"/> object.
        /// </summary>
        /// <param name="headers">The headers.</param>
        /// <returns>A list of <see cref="T:Stumps.Server.Data.NameValuePairEntity"/> objects.</returns>
        private static List<NameValuePairEntity> CreateNameValuePairFromHeaders(IHttpHeaders headers)
        {

            var pairs = new List<NameValuePairEntity>();

            foreach (var headerName in headers.HeaderNames)
            {
                var pair = new NameValuePairEntity
                {
                    Name = headerName,
                    Value = headers[headerName]
                };
                pairs.Add(pair);
            }

            return pairs;

        }

    }

}
