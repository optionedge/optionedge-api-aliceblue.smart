using OptionEdge.API.AliceBlue.Records;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OptionEdge.API.AliceBlue.Smart
{
    public class AliceBlueSmart : AliceBlue
    {
        Dictionary<string, IList<Contract>> _masterContracts = new Dictionary<string, IList<Contract>>();
        Dictionary<string, Dictionary<string, Contract>> _masterContractsSymbolToInstrumentMap = new Dictionary<string, Dictionary<string, Contract>>();
        Dictionary<string, Dictionary<int, Contract>> _masterContractsTokenToInstrumentMap = new Dictionary<string, Dictionary<int, Contract>>();

        bool _enableLogging = false;

        public AliceBlueSmart(string userId, string apiKey, string baseUrl = null, string websocketUrl = null, bool enableLogging = false, Action<string> onAccessTokenGenerated = null, Func<string> cachedAccessTokenProvider = null) : base(userId, apiKey, baseUrl, websocketUrl, enableLogging, onAccessTokenGenerated, cachedAccessTokenProvider)
        {
            _enableLogging = enableLogging;
        }

        public override Task<IList<Contract>> GetMasterContracts(string exchange)
        {
            if (_masterContracts.ContainsKey(exchange)) return Task.FromResult(_masterContracts[exchange]);

            _masterContracts.Add(exchange, base.GetMasterContracts(exchange).Result);

            return Task.FromResult(_masterContracts[exchange]);
        }

        public void LoadContracts(string exchange)
        {
            if (_masterContracts.ContainsKey(exchange))
            {
                if (_enableLogging) Utils.LogMessage($"Contracts for exchange {exchange} already loaded.");
            }

            _masterContracts.Add(exchange, base.GetMasterContracts(exchange).Result);

            UpdateCntractsMap(exchange, _masterContracts[exchange]);
        }

        public Contract GetInstrument(string exchange, string tradingSymbol)
        {
            if (!_masterContracts.ContainsKey(exchange))
                throw new Exception($"Contracts not available for exchange {exchange}.");

            return _masterContractsSymbolToInstrumentMap[exchange][tradingSymbol];
        }

        public Contract GetInstrument(string exchange, int instrumentToken)
        {
            if (!_masterContracts.ContainsKey(exchange))
                throw new Exception($"Contracts not available for exchange {exchange}.");

            return _masterContractsTokenToInstrumentMap[exchange][instrumentToken];
        }

        public bool Login()
        {
            return false;
        }

        private void UpdateCntractsMap(string exchange, IList<Contract> contracts)
        {
            if (_masterContractsSymbolToInstrumentMap.ContainsKey(exchange))
                if (_enableLogging) Utils.LogMessage("Symbol map is alreaded updated.");

            var symbolToInstrumentMap = new Dictionary<string, Contract>();
            var tokenToInstrumentMap = new Dictionary<int, Contract>();

            foreach (var contract in contracts)
            {
                if (symbolToInstrumentMap.ContainsKey(contract.TradingSymbol)) continue;

                symbolToInstrumentMap.Add(contract.TradingSymbol, contract);
                tokenToInstrumentMap.Add(contract.InstrumentToken, contract);
            }

            _masterContractsSymbolToInstrumentMap.Add(exchange, symbolToInstrumentMap);
            _masterContractsTokenToInstrumentMap.Add(exchange, tokenToInstrumentMap);
        }
    }
}
