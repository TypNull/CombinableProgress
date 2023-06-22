# Combinable Progress 
### Class for Calculating Average of Numeric Progress Objects in C# .NET 7

The Combinable Progress class is a C# .NET 7 implementation that allows for simple calculation and merging of multiple Progress objects that contain numeric values. This class uses the generic maths function introduced in .NET 7 to calculate the average of all Progress objects when they are numbers.

## Example

The project includes the Program class, which simulates a run of several Progress objects. You can exchange the type, quantity, and maximum number that the progress should return.

```cs
public static int MaxValue { get; } = 500; // for float 1 or for int 100
public static int ProgressLength { get; } = 5;

private static async Task Main(string[] args)
{
    await Run<byte>(); //  Run<decimal>(); or Run<int>(); or ...
}
```
The above code will run a simulation of five Progress objects, each with a maximum value of 500. You can also run the simulation with other numeric types such as decimal or int.

![Test](https://user-images.githubusercontent.com/70847870/233017631-04d02668-4dc5-4e18-a16c-2adeaa6c75ed.gif)

## Installation

To install and use the Combinable Progress class, simply download the code and use it in your C# .NET 7 project.

## Development

If you would like to contribute to the development of the Combinable Progress class, please follow these guidelines:

- Submit bug reports or feature requests via the issue tracker on GitHub.
- Submit code changes via pull requests on GitHub.
- Follow the coding standards and guidelines for the project.

## License

The Combinable Progress class is released under the MIT license, which allows for free use, modification, and distribution of the code.

**Free Code**🥳