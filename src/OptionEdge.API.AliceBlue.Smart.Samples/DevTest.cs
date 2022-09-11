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
        string? _mpin = Environment.GetEnvironmentVariable("ALICE_BLUE_MPIN");

        string _tokenFileName = "token.txt";

        AliceBlueSmart? _aliceBlueSmart;

        public void Run()
        {
            _aliceBlueSmart = new AliceBlueSmart(_userId, _apiKey, enableLogging: true,
                onAccessTokenGenerated: (token) =>
                {
                    File.WriteAllText(_tokenFileName, token);
                }, cachedAccessTokenProvider: () =>
                {
                    return File.Exists(_tokenFileName) ? File.ReadAllText(_tokenFileName) : "";                }
            );

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

            // Test connectivity
            var accountDetails = _aliceBlueSmart.GetAccountDetails();

            // Load all contracts for NFO in memory
            _aliceBlueSmart.LoadContracts(Constants.EXCHANGE_NFO);

            // get contract by instrument token. 
            var contract1 = _aliceBlueSmart.GetInstrument(Constants.EXCHANGE_NFO, 51942);

            // get contract by trading symbol
            var contract2 = _aliceBlueSmart.GetInstrument(Constants.EXCHANGE_NFO, "symbol");

            // Login to ANT Web 
            // set showBrowser as true to see the browser UI else false. No browser instance will be created
            // Chromium browser needs to be installed at the binary location
            //  .\playwright.ps1 install chromium
            var isLoginSuccess = _aliceBlueSmart.Login(
                userName: _userId,
                password: _password,
                mpin: _mpin,
                showBrowser: false).Result;

            if (!isLoginSuccess)
                Console.WriteLine("Logged in to ANT Web successfully.");
            else
            {
                Console.WriteLine("login to ANT Web failed.");
            }
        }
    }
}
