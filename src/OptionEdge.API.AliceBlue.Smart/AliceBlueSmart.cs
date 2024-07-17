using OptionEdge.API.AliceBlue.Records;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Playwright;
using OptionEdge.API.AliceBlue.Smart.Records;

namespace OptionEdge.API.AliceBlue.Smart
{
    public class AliceBlueSmart : AliceBlue
    {
        Dictionary<string, IList<Contract>> _masterContracts = new Dictionary<string, IList<Contract>>();
        Dictionary<string, Dictionary<string, Contract>> _masterContractsSymbolToInstrumentMap = new Dictionary<string, Dictionary<string, Contract>>();
        Dictionary<string, Dictionary<int, Contract>> _masterContractsTokenToInstrumentMap = new Dictionary<string, Dictionary<int, Contract>>();
       

        bool _enableLogging = false;
        string _antWebUrl = "https://a3.aliceblueonline.com/";

        Ticker _ticker;

        bool _isLoggedIn = false;

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
                throw new Exception($"Contracts not available for exchange {exchange}. Please make sure that contracts are loaded. Have you executed 'LoadContracts'?");

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

        Dictionary<int, LTPResult> _tickStore = new Dictionary<int, LTPResult>();
        public LTPResult GetLTP(string exchange, string tradingSymbol)
        {
            var instrument = GetInstrument(exchange, tradingSymbol);
            if (instrument == null) return null;

            var instrumentToken = instrument.InstrumentToken;

            return GetLTP(exchange, instrumentToken);
        }

        public LTPResult GetLTP(string exchange, int instrumentToken)
        {
            if (_tickStore.ContainsKey(instrumentToken))
                return _tickStore[instrumentToken];

            var oiResult = base.GetOpenInterest(exchange, new int[] { instrumentToken });
            if (oiResult == null) return null;
            if (oiResult.Length == 0) return null;

            var openInterest = oiResult[0];

            if (!_pendingSubscriptions.ContainsKey(instrumentToken))
                _pendingSubscriptions.Add(instrumentToken, new SubscriptionToken
                {
                    Exchange = exchange,
                    Token = instrumentToken
                });

            var ltpResult = new LTPResult
            {
                InstrumentToken = instrumentToken,
                LastTradedPrice = openInterest.Ltp,
                BuyPrice1 = openInterest.BestBuyPrice,
                SellPrice1 = openInterest.BestSellPrice,
                BuyQty1 = int.TryParse(openInterest.BestBuySize, out int bestBuyQty) ? bestBuyQty : 0,
                SellQty1 = int.TryParse(openInterest.BestSellSize, out int bestSellQty) ? bestSellQty : 0
            };
            _tickStore.Add(instrumentToken, ltpResult);

            if (_ticker != null)
                _ticker.Subscribe(exchange, Constants.TICK_MODE_QUOTE, new int[] { instrumentToken });
            else
                SetupTicker();

            return ltpResult;
        }

        Dictionary<int, SubscriptionToken> _pendingSubscriptions = new Dictionary<int, SubscriptionToken>();
        private async void SetupTicker()
        {
            if (_enableLogging)
                Utils.LogMessage("Setting up ticker...");

            if (_ticker != null)
            {
                if (!_ticker.IsConnected)
                {
                    _ticker.Connect();
                    return;
                }
            }

            _ticker = CreateTicker();

            _ticker.OnClose += _ticker_OnClose;
            _ticker.OnReady += _ticker_OnReady;
            _ticker.OnTick += _ticker_OnTick;

            base.SetShouldUnSubscribeHandler((instrumentToken) =>
            {
                if (_tickStore.ContainsKey(instrumentToken))
                    return false;
                else
                    return true;
            });

            _ticker.Connect();
        }

        private void _ticker_OnReady()
        {
            if (_enableLogging)
                Utils.LogMessage($"Subscribing to {_pendingSubscriptions.Count} token on Ticker On Ready.");

            _ticker.Subscribe(Constants.TICK_MODE_QUOTE, _pendingSubscriptions.Values.ToArray());
            ClearPendingSubscriptions();
        }

        private void _ticker_OnTick(Tick TickData)
        {
            if (_tickStore.ContainsKey(Convert.ToInt32( TickData.Token)))
            {
                var ltpResult = _tickStore[Convert.ToInt32(TickData.Token)];
                ltpResult.LastTradedPrice = TickData.LastTradedPrice;
                ltpResult.BuyPrice1 = TickData.BuyPrice1;
                ltpResult.SellPrice1 = TickData.SellPrice1;
            }
        }

        private void _ticker_OnClose()
        {
            ClearPendingSubscriptions();
            _tickStore.Clear();
        }

        object _pendingSubscriptionsLock = new object();
        private void ClearPendingSubscriptions()
        {
            lock (_pendingSubscriptionsLock)
                _pendingSubscriptions.Clear();
        }

        public async Task<bool> Login(string userName, string password, string yob, string mpin, bool showBrowser = true)
        {
            if (_isLoggedIn) return false;

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

                var passwordInput = page.Locator("//*[@id=\"app\"]/div/div[1]/div[2]/div/div[1]/div[2]/form/div/div[1]/span[1]/input");
                string prompt = await passwordInput.GetAttributeAsync("placeholder");

                if (prompt.Trim() == "Enter your M-Pin")
                {
                    // MPIN FLOW
                    var mpinInput = page.Locator("//*[@id=\"app\"]/div/div[1]/div[2]/div/div[1]/div[2]/form/div/div[1]/span[1]/input");
                    if (mpinInput != null)
                    {
                        await mpinInput.FillAsync(mpin);
                        await page.Locator("//*[@id=\"app\"]/div/div[1]/div[2]/div/div[1]/div[2]/form/button").ClickAsync();
                    }
                }
                else
                {
                    // PASSWORD FLOW:
                    await passwordInput.FillAsync(password);
                    await page.Locator("//*[@id=\"app\"]/div/div[1]/div[2]/div/div[1]/div[2]/form/button").ClickAsync();

                    // Fill YOB
                    await page.Locator("//*[@id=\"app\"]/div/div[1]/div[2]/div/div[1]/div[2]/form/div/div[1]/span[1]/input").FillAsync(yob);
                    await page.Locator("//*[@id=\"app\"]/div/div[1]/div[2]/div/div[1]/div[2]/form/button").ClickAsync();
                }

                await page.Locator("//*[@id=\"app\"]/div/header/div/div/div[2]/div/div[1]/div[1]").ClickAsync();

                await browser.CloseAsync();

                playwright.Dispose();

                _isLoggedIn = true;

                return _isLoggedIn;
            }
            catch (Exception ex)
            {
                if (_enableLogging)
                    Utils.LogMessage($"Error while login to broker account. {ex.ToString()}");
            }

            return _isLoggedIn;
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
