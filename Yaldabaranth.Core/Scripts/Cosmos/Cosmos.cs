using System;
using ILGPU;
using ILGPU.Runtime;

namespace Yaldabaranth.Core.Scripts.Cosmos;

public class Cosmos : IDisposable
{
  readonly YaldabaranthGame game;
  readonly Context context;
  readonly Accelerator accelerator;
  public Cosmos(YaldabaranthGame game)
  {
    this.game = game;
    var rnd = new Random();
    context = Context.Create(builder => builder
        .Default()
        .EnableAlgorithms());
    accelerator = context.GetPreferredDevice(preferCPU: false).CreateAccelerator(context);
    Console.WriteLine($"Accelerating on: {accelerator.Name}");
    Console.WriteLine("Generating initial stars.");
  }
  public void Update()
  {
  }
  public void Blit()
  {
  }
  public void Dispose()
  {
    context.Dispose();
    accelerator.Dispose();
  }
}
