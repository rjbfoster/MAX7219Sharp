using MAX7219Sharp;

/* This is an example of how to use the MAX7219 library. It will turn the display on,
 * write the word 'HELLO', clear the display, and then scroll the word 'BYE' for 10 seconds.
 * 
 * To deploy this project to a Raspberry Pi, follow these steps:
 * 
 * 1) Ensure your MAX7219 controlled display is properly connected.
 * 
 * 2) Publish the project using the following settings as a guide (you may need to change this):
 *      - Deployment mode:  Self contained
 *      - Target runtime:   linux-arm64
 *      
 * 3) Upload the published files to your Raspberry Pi.
 * 
 * 4) Once uploaded, give your Raspberry Pi permission to execute the file by running the following command:
 *       chmod +x DisplayDemo
 *       
 * 5) Run the program by executing the following command:
 *       ./DisplayDemo
 */

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

// Scroll the word 'BYE'. By default, it will scroll for 60 seconds if no time is specified.
// You can specify a different time, as has been done with the overload '10' here:
max7219.Scroll("BYE", 10);

Console.WriteLine("Press any key to exit");
Console.ReadKey();