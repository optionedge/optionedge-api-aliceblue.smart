using System;
using System.Collections.Generic;
using System.Linq;

namespace OptionEdge.API.AliceBlue.Smart.Samples
{
    public class DevTest
    {
        string _userId = Environment.GetEnvironmentVariable("ALICE_BLUE_USER_ID");
        string _apiKey = Environment.GetEnvironmentVariable("ALICE_BLUE_API_KEY");

        string _tokenFileName = "token.txt";

        AliceBlueSmart _aliceBlueSmart;

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

            //var accountDetails = _aliceBlueSmart.GetAccountDetails();
            //var funds = _aliceBlueSmart.GetFunds();

            // var masterContractsNFO = _aliceBlueSmart.GetMasterContracts(Constants.EXCHANGE_NFO).Result;

            _aliceBlueSmart.LoadContracts(Constants.EXCHANGE_NFO);

            var contract = _aliceBlueSmart.GetInstrument(Constants.EXCHANGE_NFO, 51942);
            
        }
    }
}
