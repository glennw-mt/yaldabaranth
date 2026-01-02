using System;
using System.Collections.Generic;
using System.Linq;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Algorithms.Vectors;
using ILGPU.Runtime;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Yaldabaranth.Core.Scripts.Cosmos;

public class Cosmos : IDisposable
{
  readonly YaldabaranthGame game;
  private readonly Context context;
  private readonly Accelerator accelerator;
  readonly MemoryBuffer1D<Star, Stride1D.Dense> bufferA;
  readonly MemoryBuffer1D<Star, Stride1D.Dense> bufferB;
  readonly Action<Index1D, double, ArrayView<Star>, ArrayView<Star>> nBodyKernel;
  private List<Star> stars = [];
  private List<Star> blackHoles = [];
  private readonly double nStars = 5e3;
  private static readonly float blackHoleMass = 1e5f;
  private static readonly float starMass = 1f;
  public struct Vec2(float x, float y)
  {
    public float X = x;
    public float Y = y;
    public static Vec2 operator +(Vec2 v1, Vec2 v2) => new(v1.X + v2.X, v1.Y + v2.Y);
    public static Vec2 operator +(Vec2 v, float f) => new(v.X + f, v.Y + f);
    public static Vec2 operator -(Vec2 v1, Vec2 v2) => new(v1.X - v2.X, v1.Y - v2.Y);
    public static double operator *(Vec2 v1, Vec2 v2) => v1.X * v2.X + v1.Y * v2.Y;
    public static Vec2 operator /(Vec2 v, float f) => new(v.X / f, v.Y / f);
    public static Vec2 operator *(Vec2 v, float f) => new(v.X * f, v.Y * f);
  }
  public struct Star(Float64x2 p, Float64x2 v, float m)
  {
    public Float64x2 P = p;
    public Float64x2 V = v;
    public float mass = m;
  }
  static void NBodyKernelDef(Index1D index, double dt, ArrayView<Star> current, ArrayView<Star> next)
  {
    Star me = current[index];
    if (me.mass == blackHoleMass) { next[index] = me; return; }
    Float64x2 totalForce = new(0, 0);
    double softening = 0.5f;
    for (int i = 0; i < current.Length; i++)
    {
      if (i == index) continue;
      Star other = current[i];
      if (other.mass != blackHoleMass) continue;
      Float64x2 r = other.P - me.P;
      double rSquared = r.X * r.X + r.Y * r.Y + softening;
      double rAbs = XMath.Sqrt(rSquared);
      double forceMag = me.mass * other.mass / rSquared;
      totalForce += new Float64x2(r.X / rAbs * forceMag, r.Y / rAbs * forceMag);
    }
    Float64x2 acceleration = new(totalForce.X / me.mass, totalForce.Y / me.mass);
    next[index] = new Star(
        me.P + new Float64x2(me.V.X * dt, me.V.Y * dt),
        me.V + new Float64x2(acceleration.X * dt, acceleration.Y * dt),
        me.mass);
  }
  public Cosmos(YaldabaranthGame game)
  {
    this.game = game;
    var rnd = new Random();
    context = Context.CreateDefault();
    accelerator = context.GetPreferredDevice(preferCPU: false).CreateAccelerator(context);
    Console.WriteLine($"Accelerating on: {accelerator.Name}");

    var noiseGenerator = new FastNoiseLite(rnd.Next());
    Console.WriteLine("Generating initial stars.");
    var blackHole1 = new Star(new Float64x2(200, 300), new Float64x2(0, 0), blackHoleMass);
    blackHoles.Add(blackHole1);
    var blackHole2 = new Star(new Float64x2(600, 500), new Float64x2(0, 0), blackHoleMass);
    blackHoles.Add(blackHole2);
    var blackHole3 = new Star(new Float64x2(670, 100), new Float64x2(0, 0), blackHoleMass);
    blackHoles.Add(blackHole3);
    var blackHole4 = new Star(new Float64x2(100, 900), new Float64x2(0, 0), blackHoleMass);
    blackHoles.Add(blackHole4);
    while (stars.Count < nStars)
    {
      var randPos = new Float64x2(rnd.Next(0, 1000), rnd.Next(0, 1000));
      var noise = noiseGenerator.GetNoise((float)randPos.X, (float)randPos.Y);
      if (rnd.Next(0, 100) > 10)
      {
        var star = new Star(randPos, new Float64x2(0, 0), starMass);
        stars.Add(star);
      }
      else
      {
        var star = new Star(randPos, new Float64x2(0, 0), starMass * 10);
        stars.Add(star);
      }
    }
    var starArray = stars.ToArray();
    var blackHoleArray = blackHoles.ToArray();
    for (int i = 0; i < starArray.Length; i++)
    {
      var s = starArray[i];
      var nearestBH = blackHoles
          .OrderBy(bh =>
          {
            double dx = bh.P.X - s.P.X;
            double dy = bh.P.Y - s.P.Y;
            return dx * dx + dy * dy;
          }).First();
      double dx = s.P.X - nearestBH.P.X;
      double dy = s.P.Y - nearestBH.P.Y;
      double dist = Math.Sqrt(dx * dx + dy * dy);
      double orbitalSpeed = Math.Sqrt(blackHoleMass / dist);
      Float64x2 perpendicular = new(dy / dist, -dx / dist);
      s.V = new Float64x2(perpendicular.X * orbitalSpeed, perpendicular.Y * orbitalSpeed);
      starArray[i] = s;
    }
    starArray = [.. starArray, .. blackHoleArray];
    bufferA = accelerator.Allocate1D<Star>(starArray.Length);
    bufferB = accelerator.Allocate1D<Star>(starArray.Length);
    bufferA.CopyFromCPU(starArray);
    stars = [.. starArray];
    nBodyKernel = accelerator.LoadAutoGroupedStreamKernel<Index1D, double, ArrayView<Star>, ArrayView<Star>>(NBodyKernelDef);
  }
  public void Update()
  {
    var starArray = stars.ToArray();
    bufferA.CopyFromCPU(starArray);
    Blit(starArray);
    double deltaTime = 0.0001d;
    nBodyKernel(starArray.Length, deltaTime, bufferA.View, bufferB.View);
    accelerator.Synchronize();
    bufferB.CopyToCPU(starArray);
    stars = [.. starArray];
  }
  public void Blit(Star[] starArray)
  {
    foreach (var star in starArray)
    {
      var pos = star.P;
      if (star.mass == starMass)
        game.Canvas.DrawCircle(new Vector2((float)pos.X, (float)pos.Y), radius: 1f, sides: 2, color: Color.White);
      else if (star.mass == starMass * 10)
        game.Canvas.DrawCircle(new Vector2((float)pos.X, (float)pos.Y), radius: 2.5f, sides: 3, color: Color.Green);
      else
        game.Canvas.DrawCircle(new Vector2((float)pos.X, (float)pos.Y), radius: 10f, sides: 4, color: Color.Red);
    }
  }
  public void Dispose()
  {
    context.Dispose();
    accelerator.Dispose();
  }
}
