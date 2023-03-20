using MAX7219Sharp;

// Create a new MAX7219 object with default values
MAX7219 max7219 = new MAX7219();

Console.WriteLine("Press any key to test the display");
Console.ReadKey();

// Test the display by turning it on, waiting 1 second, and then turning it off
max7219.AllOn();
Thread.Sleep(1000);
max7219.AllOff();

Console.WriteLine("Press any key to write the word 'HELLO'");
Console.ReadKey();

// Write the word 'HELLO'
max7219.Write("HELLO");

Console.WriteLine("Press any key to clear the display");
Console.ReadKey();

// Clear the display
max7219.ClearDisplay();

Console.WriteLine("Press any key to scroll the word 'BYE' for 10 seconds");
Console.ReadKey();

// Scroll the word 'BYE'. By default, it will scroll for 60 seconds, but you can specify
// a different time as has been done with the overload '10' here:
max7219.Scroll("BYE", 10);

Console.WriteLine("Press any key to exit");
Console.ReadKey();