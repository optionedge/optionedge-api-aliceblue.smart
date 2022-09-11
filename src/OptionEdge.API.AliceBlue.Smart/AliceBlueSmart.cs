using OptionEdge.API.AliceBlue.Records;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Playwright;

namespace OptionEdge.API.AliceBlue.Smart
{
    public class AliceBlueSmart : AliceBlue
    {
        Dictionary<string, IList<Contract>> _masterContracts = new Dictionary<string, IList<Contract>>();
        Dictionary<string, Dictionary<string, Contract>> _masterContractsSymbolToInstrumentMap = new Dictionary<string, Dictionary<string, Contract>>();
        Dictionary<string, Dictionary<int, Contract>> _masterContractsTokenToInstrumentMap = new Dictionary<string, Dictionary<int, Contract>>();

        bool _enableLogging = false;
        string _antWebUrl = "https://a3.aliceblueonline.com/";

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

            return 
                _masterContractsSymbolToInstrumentMap[exchange].ContainsKey(tradingSymbol) ?
                _masterContractsSymbolToInstrumentMap[exchange][tradingSymbol] : null;
        }

        public Contract GetInstrument(string exchange, int instrumentToken)
        {
            if (!_masterContracts.ContainsKey(exchange))
                throw new Exception($"Contracts not available for exchange {exchange}.");

            return _masterContractsTokenToInstrumentMap[exchange][instrumentToken];
        }

        public async Task<bool> Login(string userName, string password, string mpin, bool showBrowser = true)
        {
            try
            {
                var playwright = await Playwright.CreateAsync();
                var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = showBrowser == true ? false : true,
                    Devtools = false
                });

                var page = await browser.NewPageAsync();
                await page.GotoAsync(_antWebUrl);

                await page.Locator("//*[@id=\"app\"]/div/div[1]/div[2]/div/div[1]/div[2]/form/div/input").FillAsync(userName);
                await page.Locator("//*[@id=\"app\"]/div/div[1]/div[2]/div/div[1]/div[2]/form/button/span").ClickAsync();

                // MPIN
                // Currently in chromium, it always opens MPIN, once user id is entered
                // Unable to verify the password & YOB login flow
                await page.Locator("//*[@id=\"app\"]/div/div[1]/div[2]/div/div[1]/div[2]/form/div/div[1]/span[1]/input").FillAsync(mpin);
                await page.Locator("//*[@id=\"app\"]/div/div[1]/div[2]/div/div[1]/div[2]/form/button").ClickAsync();

                await page.Locator("//*[@id=\"app\"]/div/header/div/div/div[2]/div/div[1]/div[1]").ClickAsync();

                await browser.CloseAsync();

                playwright.Dispose();

                return true;
            }
            catch (Exception ex)
            {
                if (_enableLogging)
                    Utils.LogMessage($"Error while login to broker account. {ex.ToString()}");
            }

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

        public ExpiryCalculator CreateExpiryCalculator(DateTime today)
        {
            return new ExpiryCalculator(today, null);
        }

        public SymbolGenerator CreateSymbolGenerator()
        {
            return new SymbolGenerator();
        }
    }
}
