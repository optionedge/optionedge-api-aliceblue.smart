using System;
using System.Collections.Generic;
using System.Linq;

namespace OptionEdge.API.AliceBlue.Smart.Samples
{
    public class DevTest
    {
        string? _userId = Environment.GetEnvironmentVariable("ALICE_BLUE_USER_ID");
        string? _apiKey = Environment.GetEnvironmentVariable("ALICE_BLUE_API_KEY");

        string? _password = Environment.GetEnvironmentVariable("ALICE_BLUE_PASSWORD");
        string? _yob = Environment.GetEnvironmentVariable("ALICE_BLUE_YOB");
        string? _mpin = Environment.GetEnvironmentVariable("ALICE_BLUE_MPIN");

        string _tokenFileName = $"token_{DateTime.Now.ToString("dd_MMM_yyyy")}.txt";

        AliceBlueSmart? _aliceBlueSmart;

        public void Run()
        {
            _aliceBlueSmart = new AliceBlueSmart(_userId, _apiKey, enableLogging: true,
                onAccessTokenGenerated: (token) =>
                {
                    File.WriteAllText(_tokenFileName, token);
                }, cachedAccessTokenProvider: () =>
                {
                    return File.Exists(_tokenFileName) ? File.ReadAllText(_tokenFileName) : "";                
                }
            );

            ////Login to ANT Web
            //// set showBrowser as true to see the browser UI else false.No browser instance will be created
            ////Chromium browser needs to be installed at the binary location
            //// .\playwright.ps1 install chromium
            //var isLoginSuccess = _aliceBlueSmart.Login(
            //    userName: _userId,
            //    password: _password,
            //    yob: _yob,
            //    mpin: _mpin,
            //    showBrowser: true).Result;

            //if (isLoginSuccess)
            //    Console.WriteLine("Logged in to ANT Web successfully.");
            //else
            //{
            //    Console.WriteLine("login to ANT Web failed.");
            //}

            // Create Ticker for live feeds
            var ticker = _aliceBlueSmart.CreateTicker();
            ticker.OnConnect += Ticker_OnConnect;

            // Subscribe for live feeds by instrument token 
            ticker.Subscribe(Constants.EXCHANGE_NFO, Constants.TICK_MODE_QUOTE, new int[] { 2222 });

            // UnSubscribe from live feeds
            ticker.UnSubscribe(Constants.EXCHANGE_NFO, new int[] { 2222 });

            var expiryCalculator = _aliceBlueSmart.CreateExpiryCalculator(DateTime.Now);

            var allExpiries = expiryCalculator.GetExpiries();
            var monthlies = expiryCalculator.GetMonthlies();

            var symbolGenerator = _aliceBlueSmart.CreateSymbolGenerator();

            // Get NIFTY current week's put ATM strike symbol
            var niftyCurrentWeekATMPut = symbolGenerator.GetSymbol(
                "NIFTY",
                allExpiries[0],
                ALICE_BLUE_API_OPTION_TYPE.PE,
                symbolGenerator.GetATMStrike(17343, 50));


            var bankniftyNextWeekATMCall = symbolGenerator.GetSymbol(
                "BANKNIFTY",
                allExpiries[1],
                ALICE_BLUE_API_OPTION_TYPE.CE,
                symbolGenerator.GetATMStrike(39856, 100));

            Console.WriteLine(niftyCurrentWeekATMPut);
            Console.WriteLine(bankniftyNextWeekATMCall);

            // Test connectivity
            var accountDetails = _aliceBlueSmart.GetAccountDetails();

            // Load all contracts for NFO in memory
            _aliceBlueSmart.LoadContracts(Constants.EXCHANGE_NFO);

            // get contract by instrument token. 
            var contract1 = _aliceBlueSmart.GetInstrument(Constants.EXCHANGE_NFO, 51942);

            // get contract by trading symbol
            var contract2 = _aliceBlueSmart.GetInstrument(Constants.EXCHANGE_NFO, "symbol");
            
            // Get Last Traded Price by Trading Symbol or Instrument Token
            var ltp = _aliceBlueSmart.GetLTP(Constants.EXCHANGE_NFO, "BANKNIFTY22SEP40800CE");
        }

        private void Ticker_OnConnect()
        {
            Console.WriteLine("Ticker Connected");
        }
    }
}
