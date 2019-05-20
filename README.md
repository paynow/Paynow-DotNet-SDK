# Paynow Zimbabwe .NET SDK
.NET SDK for Paynow Zimbabwe's API

## Sign in to Paynow and get integration details

> Before you can start making requests to Paynow's API, you need to get an integration ID and integration Key from Paynow. Details about how you can retrieve the ID and key are explained in detail on [this page](generation.md)

## Prerequisites

This library has a set of prerequisites that must be met for it to work

1.  .NET Framework 4.5.2 or later (although the project targets .NET Framework 4.5.2, the code is directly compatible with .NET Core 2.0 or later)

## Installation

There are two ways to install the Paynow .NET SDK. The first method uses the NuGet Package Manager, which comes preconfigured with Visual Studio 2013 and later. 

If you prefer to not use NuGet, you can also download the SDK directly from this repository.


### Installing using Nuget

From the Nuget package manager console, enter the follwing command

```sh
PM> Install-Package Paynow
```

### Downloading the SDK directly

1. Navigate to the Paynow [.NET SDK Releases page](https://gitlab.com/paynow-developer-hub/Paynow-DotNet-SDK/-/archive/1.0.0/Paynow-DotNet-SDK-1.0.0.zip)
2. Click the appropriate link to download a ZIP file containing the SDK files


## Usage example

### Importing library

```csharp
using Webdev.Payments.Paynow;
```

Create an instance of the Paynow class optionally setting the result and return url(s)

```cs
var paynow = new Paynow("INTEGRATION_ID", "INTEGRATION_KEY");

paynow.ResultUrl = "http://example.com/gateways/paynow/update";
paynow.ReturnUrl = "http://example.com/return?gateway=paynow";
// The return url can be set at later stages. You might want to do this if you want to pass data to the return url (like the reference of the transaction)
```

Create a new payment passing in the reference for that payment (e.g invoice id, or anything that you can use to identify the transaction.

```cs
var payment = paynow.CreatePayment("Invoice 35");
```

You can then start adding items to the payment

```cs
// Passing in the name of the item and the price of the item
payment.add("Bananas", 2.5);
payment.add("Apples", 3.4);
```

Once you're done building up your cart and you're finally ready to send your payment to Paynow, you can use the `Send` method in the `paynow` object.

```cs
// Save the response from paynow in a variable
paynow.Send(payment);
```

The send method will return an instance of the `InitResponse` class, the InitResponse object being the response from Paynow and it will contain some useful information like whether the request was successful or not. If it was, for example, it contains the url to redirect the user so they can make the payment. You can view the full list of data contained in the response in our wiki

If request was successful, you should consider saving the poll url sent from Paynow in your database

```cs
var response = paynow.Send(payment);

if(response.Success()) 
{   
    // Get the url to redirect the user to so they can make payment
    var link = response.RedirectLink();
    
    // Get the poll url of the transaction
    var pollUrl = response.PollUrl(); 
}
```

# Mobile Transactions

If you want to send an express (mobile) checkout request instead, when creating a payment you make a call to an overload of the `CreatePayment` method.This overload takes in two arguments, the reference of the transaction followed by the payer's email address. **Mobile transactions** require that you pass in the email address of the user making the payment. The email address is used by Paynow to email a payment summary and reference to the person who has made payment. Its specifically for the customer, not the merchant. However, there’s nothing stopping you using your own email address if you don’t mind the customer not getting a copy of the payment.

Additionally, you send the payment to Paynow by making a call to the `SendMobile` in the `paynow` object
instead of the `Send` method. The `SendMobile` method unlike the `Send` method takes in two additional arguments i.e The phone number to send the payment request to and the mobile money method to use for the request. **Note that currently only ecocash is supported**

**Note: The payer's email address is required for mobile transactions**

```cs
// Create a mobile payment, passing the payer's email address. 
var payment = paynow.CreatePayment("Invoice 32", "user@example.com");

// Add items to the payment
payment.Add("Bananas", 2.5);
payment.Add("Apples", 3.4);

// Send the payment to paynow
paynow.SendMobile(payment)
```

The response object is almost identical to the one you get if you send a normal request. With a few differences, firstly, you don't get a url to redirect to. Instead you instructions (which ideally should be shown to the user instructing them how to make payment on their mobile phone)

```csharp
var response = paynow.SendMobile(payment);

// Check if request was successful
if(response.Success()) 
{   
    // Get the url to redirect the user to so they can make payment
    var link = response.RedirectLink();
    
    // Get the poll url (used to check the status of a transaction). You might want to save this in your DB
    var pollUrl = response.PollUrl(); 
    
    // Get the instructions
    var instructions =  response.Instructions();
}
else
{
    // Ahhhhhhhhhhhhhhh
    // *freak out*
}
```

# Checking transaction status

The SDK exposes a handy method that you can use to check the status of a transaction. Once you have instantiated the Paynow class.

```cs
// Check the status of the transaction with the specified pollUrl
// Now you see why you need to save that url ;-)
var status = paynow.CheckTransactionStatus(pollUrl);

if (status.Paid()) {
  // Yay! Transaction was paid for
} else {
  Console.WriteLine("Why you no pay?");
}
```

## Full Usage Example

```cs
// Require in the Paynow class
using System;
using Webdev.Payments.Paynow;


class Program
{
    public static void Main(string[] args)
    {
        // Create an instance of the paynow class
        var paynow = new Paynow("INTEGRATION_ID", "INTEGRATION_KEY");

        paynow.ResultUrl = "http://example.com/gateways/paynow/update";
        paynow.ReturnUrl = "http://example.com/return?gateway=paynow";
        // The return url can be set at later stages. You might want to do this if you want to pass data to the return url (like the reference of the transaction)
            
        // Create a new payment 
        var payment = paynow.CreatePayment("Invoice 35");
    
        // Add items to the payment
        payment.Add("Bananas", 2.5);
        payment.Add("Apples", 3.4);
        
        // Send payment to paynow
        var response = paynow.Send(payment);
    
        // Check if payment was sent without error
        if(response.Success())  
        {   
            // Get the url to redirect the user to so they can make payment
            var link = response.RedirectLink();
            
            // Get the poll url of the transaction
            var pollUrl = response.PollUrl(); 
        }
    }
}   
```
