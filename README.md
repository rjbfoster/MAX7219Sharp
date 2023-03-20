# MAX7219 Sharp
This is a class library which facilitates easy manipulation of LED displays driven by the MAX7219 chip over a Serial Peripheral interface (SPI). The library is designed to be lightweight and is targeted at .NET 7, utilizing Microsoft's System.Device.Spi library to interface with the device.

## Purpose

The purpose of this program is to enable the quick and easy usage of off-the-shelf MAX7129 displays, primarily on platforms such as the Raspberry Pi.

## Usage

Download and install the package using NuGet:

```dotnet add package MAX7219Sharp --version 1.0.0```

You can also use the Package Manager Console:

```PM> NuGet\Install-Package MAX7219Sharp -Version 1.0.0```

### Example

A simple example of how to use the library is shown below:

```csharp
using MAX7219Sharp;

Console.WriteLine("Writing HELLO to display");
var device = new MAX7219();

device.Write("HELLO");

Thread.Sleep(1000);

device.ClearDisplay();

Console.WriteLine("Press any key to quit");
Console.ReadKey();
```

Please see the DisplayDemo project for a more in-depth example, and/or to provide a starting point for your own code.