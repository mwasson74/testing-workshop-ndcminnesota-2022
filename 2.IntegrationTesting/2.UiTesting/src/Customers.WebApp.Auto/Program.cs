﻿
using Microsoft.Playwright;

IPlaywright playwright = await Playwright.CreateAsync();

IBrowser browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
{
    SlowMo = 1500,
    Headless = false
});

IBrowserContext browserContext = await browser.NewContextAsync(new BrowserNewContextOptions
{
    IgnoreHTTPSErrors = true
});

IPage page = await browserContext.NewPageAsync();

await page.GotoAsync("https://localhost:5001/add-customer");

await page.Locator("id=fullname").FillAsync("Nick Chapsas");
await page.Locator("[type='email']").FillAsync("nick@chapsas.com");
await page.Locator("id=github-username").FillAsync("nickchapsas");
await page.Locator("id=dob").FillAsync("1993-01-01");
await page.Locator("text='Submit'").ClickAsync();

playwright.Dispose();
