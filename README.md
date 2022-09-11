# Smart Extensions for AliceBlue .Net Client Library 

This is an extension library to AliceBlue .Net Client library. This smart extension adds following useful features:

- Auto login (through PlayWright)
- Search contracts/instruments by instrument symbol or instrument token.
- Expiry calculator: get weekly/monthly expiry dates
- Symbol Generator: generate trading symbol based on expiry, strike and option type
- Many more features coming soon

## Refer to this Youtube video for AliceBlue .Net Client library usage & integration guide  [ProfTheta - Your Guide to Options](https://www.youtube.com/watch?v=ncjVPPeSQ88)

[![Watch the video](https://img.youtube.com/vi/ncjVPPeSQ88/mqdefault.jpg)](https://www.youtube.com/watch?v=ncjVPPeSQ88)

## Requirements

- Targets `netstandard2.0` and can be used with `.Net Core 2.0 and above` & `.Net Framework 4.6 and above`.
- Auto login feature is provided using PlayWright through the Chromium browser. You may need to download the Chromium broswer to the application folder through the following command:

```
 .\playwright.ps1 install chromium
```

## Install Nuget Packages

```
Install-Package OptionEdge.API.AliceBlue -Version 1.0.0.5-beta
Install-Package OptionEdge.API.AliceBlue.Smart -Version 1.0.0.5-beta
```

## Sample project
Refer the sample file `DevTest.cs` within `OptionEdge.API.AliceBlue.Smart.Samples` project which demonstrate the capabilities of this library.

## Getting Started

## Import namespaces
```csharp
    using OOptionEdge.API.AliceBlue.Smart;
    using OptionEdge.API.AliceBlue.Records;
```

## Sample Class File
```csharp

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
                    return File.Exists(_tokenFileName) ? File.ReadAllText(_tokenFileName) : "";                
                }
            );

            // YOUR CODE HERE
            // SEE BELOW FOR SAMPLE CODE
        }
    }

```

## Auto login
```csharp
    // Login to ANT Web 
    // set showBrowser as true to see the browser UI else false. No browser UI  will be created
    // Chromium browser needs to be installed at the binary location
    //  .\playwright.ps1 install chromium
    var isLoginSuccess = _aliceBlueSmart.Login(
        userName: _userId,
        password: _password,
        mpin: _mpin,
        showBrowser: false).Result;

    if (isLoginSuccess)
        Console.WriteLine("Logged in to ANT Web successfully.");
    else
    {
        Console.WriteLine("login to ANT Web failed.");
    }
```

## Expiry Calculator
```csharp
    var expiryCalculator = _aliceBlueSmart.CreateExpiryCalculator(DateTime.Now);

    var allExpiries = expiryCalculator.GetExpiries();
    var monthlies = expiryCalculator.GetMonthlies();
```

### Expiry Calculator Output
```console
    15-09-2022
    22-09-2022
    29-09-2022
    27-10-2022
    24-11-2022
```

## Symbol Generator
```csharp
    var symbolGenerator = _aliceBlueSmart.CreateSymbolGenerator();

    // Get NIFTY current week's put ATM strike symbol
    var niftyCurrentWeekATMPut = symbolGenerator.GetSymbol(
        "NIFTY", 
        allExpiries[0], 
        ALICE_BLUE_API_OPTION_TYPE.PE, 
        symbolGenerator.GetATMStrike(17343, 50));
```

Above code generates `NIFTY2291517350PE` & `BANKNIFTY2292239900CE` symbols.

# Links
- [AliceBlue API .Net Client Library](https://github.com/proftheta/optionedge-api-aliceblue)
- [AliceBlue V2 API Documentation](https://v2api.aliceblueonline.com/introduction)
- [AliceBlue V2 API Postman Collection](https://v2api.aliceblueonline.com/Aliceblue.postman_collection.json)
- [AliceBlue Ant Web](https://a3.aliceblueonline.com/)
- [ProfTheta Twitter @ProfTheta21](https://twitter.com/ProfTheta21)
- [ProfTheta Youtube Channel](https://www.youtube.com/channel/UChp2hjl-OgGpHKCrwJPohEQ)

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)