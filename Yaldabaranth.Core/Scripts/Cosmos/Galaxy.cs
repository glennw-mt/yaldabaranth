using System;
using System.Collections.Generic;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Algorithms.Vectors;
using ILGPU.Runtime;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Yaldabaranth.Core.Scripts.Cosmos;

public class GalaxyParams
{
  public const double Width = 4000f;
  public const double Height = 2000f;

  public const int NStars = 1000;
  public const double StarMass = 1f;
  public const double BlackHoleMass = 1e5f;

  public const double Dt = 0.0001;
  public const double Softening = 0.5;

  public const double OmegaBar = 0.5;
  public const double BarAmplitude = 0.1;
  public const double BarRadius = 50f;

  public const double SpiralM = 2.0;
  public const double SpiralPitch = 0.2;
  public const double OmegaSpiral = 1.0;
  public const double SpiralAmplitude = 0.5;
}
public struct Star(Float64x2 p, Float64x2 v, double mass)
{
  public Float64x2 P = p;
  public Float64x2 V = v;
  public double Mass = mass;
}
public static class GalaxyHelpers
{
  public static Float64x2 BarForce(Float64x2 p, double time)
  {
    double angle = GalaxyParams.OmegaBar * time;
    double cosA = XMath.Cos(angle);
    double sinA = XMath.Sin(angle);

    double xr = p.X * cosA + p.Y * sinA;
    double yr = -p.X * sinA + p.Y * cosA;

    double r = XMath.Sqrt(xr * xr + yr * yr + 1e-6);
    double amp = r <= GalaxyParams.BarRadius
      ? GalaxyParams.BarAmplitude * (r / GalaxyParams.BarRadius)
      : GalaxyParams.BarAmplitude * XMath.Pow(GalaxyParams.BarRadius / r, 3);
    double fxr = -2.0 * amp * xr;
    double fyr = 2.0 * amp * yr;
    return new Float64x2(
      fxr * cosA - fyr * sinA,
      fxr * sinA + fyr * cosA
    );
  }
  public static Float64x2 SpiralForce(Float64x2 p, double time)
  {
    double r = XMath.Max(XMath.Sqrt(p.X * p.X + p.Y * p.Y), 0.1);
    double phi = XMath.Atan2(p.Y, p.X);

    double phase =
        GalaxyParams.SpiralM *
        (phi - GalaxyParams.OmegaSpiral * time
        - XMath.Log(r) / XMath.Tan(GalaxyParams.SpiralPitch));

    double fr = GalaxyParams.SpiralAmplitude * XMath.Cos(phase);
    double fa = GalaxyParams.SpiralAmplitude * XMath.Sin(phase);

    return new Float64x2(
        p.X / r * fr - p.Y / r * fa,
        p.Y / r * fr + p.X / r * fa
    );
  }
  public static Float64x2 WrapCoords(Float64x2 d, float scale)
  {
    if (d.X > GalaxyParams.Width * 0.5) d -= new Float64x2(GalaxyParams.Width * scale, 0);
    if (d.X < -GalaxyParams.Width * 0.5) d += new Float64x2(GalaxyParams.Width * scale, 0);
    if (d.Y > GalaxyParams.Height * 0.5) d -= new Float64x2(0, GalaxyParams.Height * scale);
    if (d.Y < -GalaxyParams.Height * 0.5) d += new Float64x2(0, GalaxyParams.Height * scale);
    return d;
  }
  public static void GalaxyKernel(
    Index1D i,
    double time,
    ArrayView<Star> current,
    ArrayView<Star> next)
  {
    Star me = current[i];
    if (me.Mass == GalaxyParams.BlackHoleMass)
    {
      next[i] = me;
      return;
    }

    Float64x2 force = BarForce(me.P, time) + SpiralForce(me.P, time);

    for (int j = 0; j < current.Length; j++)
    {
      Star other = current[j];
      if (other.Mass == GalaxyParams.StarMass) continue;

      Float64x2 r = WrapCoords(other.P - me.P, 0f);
      double r2 = r.X * r.X + r.Y * r.Y + GalaxyParams.Softening;
      double invR = XMath.Rsqrt(r2);

      double f = me.Mass * other.Mass * invR * invR;
      double scale = f * invR;
      force += new Float64x2(r.X * scale, r.Y * scale);
    }
    Float64x2 a = new(
        force.X / me.Mass,
        force.Y / me.Mass
    );

    Float64x2 newP = new(
      me.P.X + me.V.X * GalaxyParams.Dt,
      me.P.Y + me.V.Y * GalaxyParams.Dt
    );
    newP = WrapCoords(newP, 0.5f);

    next[i] = new Star(
        newP,
        new Float64x2(me.V.X + a.X * GalaxyParams.Dt, me.V.Y + a.Y * GalaxyParams.Dt),
        me.Mass
    );
  }
}

public class Galaxy : IDisposable
{
  readonly YaldabaranthGame game;
  private readonly Context context;
  private readonly Accelerator accelerator;
  MemoryBuffer1D<Star, Stride1D.Dense> bufferA;
  MemoryBuffer1D<Star, Stride1D.Dense> bufferB;
  readonly Action<Index1D, double, ArrayView<Star>, ArrayView<Star>> galaxyKernel;
  private readonly Star[] stars = [];
  private readonly Star[] blackHoles = [];
  static double time = 0f;
  public Galaxy(YaldabaranthGame game)
  {
    this.game = game;
    var rnd = new Random();
    context = Context.Create(builder => builder
        .Default()
        .EnableAlgorithms());
    accelerator = context.GetPreferredDevice(preferCPU: false).CreateAccelerator(context);
    Console.WriteLine($"Accelerating on: {accelerator.Name}");
    Console.WriteLine("Generating initial stars.");
    var starList = new List<Star>();
    var blackHoleList = new List<Star>();
    var noiseGenerator = new FastNoiseLite(rnd.Next());
    var blackHole = new Star(new Float64x2(0, 0), new Float64x2(0, 0), GalaxyParams.BlackHoleMass);
    blackHoleList.Add(blackHole);
    double scaleLength = GalaxyParams.Width / 32;
    while (starList.Count < GalaxyParams.NStars)
    {
      var parentPos = blackHoleList[rnd.Next(blackHoleList.Count)].P;
      double u = rnd.NextDouble();
      double r = -scaleLength * Math.Log(1.0 - u) + 2f;
      double theta = rnd.NextDouble() * 2.0 * Math.PI;
      double x = parentPos.X + r * Math.Cos(theta) * 1.25f;
      double y = parentPos.Y + r * Math.Sin(theta) * 0.75f;
      x = ((x % GalaxyParams.Width + GalaxyParams.Width * 1.5) % GalaxyParams.Width) - GalaxyParams.Width / 2;
      y = ((y % GalaxyParams.Height + GalaxyParams.Height * 1.5) % GalaxyParams.Height) - GalaxyParams.Height / 2;
      Float64x2 finalPos = new(x, y);
      var star = new Star(finalPos, new Float64x2(0, 0), GalaxyParams.StarMass);
      starList.Add(star);
    }
    stars = [.. starList, .. blackHoleList];
    blackHoles = [.. blackHoleList];
    for (int i = 0; i < stars.Length; i++)
    {
      var s = stars[i];
      var nearestBH = blackHoles[0];
      double minSDist = double.MaxValue;
      Float64x2 bestR = new(0, 0);
      foreach (var bh in blackHoles)
      {
        Float64x2 r = s.P - bh.P;
        if (r.X > GalaxyParams.Width / 2) r -= new Float64x2(GalaxyParams.Width, 0);
        else if (r.X < -GalaxyParams.Width / 2) r += new Float64x2(GalaxyParams.Width, 0);
        if (r.Y > GalaxyParams.Height / 2) r -= new Float64x2(0, GalaxyParams.Height);
        else if (r.Y < -GalaxyParams.Height / 2) r += new Float64x2(0, GalaxyParams.Height);
        double sDist = r.X * r.X + r.Y * r.Y;
        if (sDist < minSDist)
        {
          minSDist = sDist;
          bestR = r;
        }
      }
      double dist = Math.Sqrt(minSDist);
      double orbitalSpeed = Math.Sqrt(GalaxyParams.BlackHoleMass / dist);
      Float64x2 unitPerp = new(-bestR.Y / dist, bestR.X / dist);
      s.V = new Float64x2(unitPerp.X * orbitalSpeed, unitPerp.Y * orbitalSpeed);
      stars[i] = s;
    }
    bufferA = accelerator.Allocate1D<Star>(stars.Length);
    bufferB = accelerator.Allocate1D<Star>(stars.Length);
    bufferA.CopyFromCPU(stars);
    galaxyKernel = accelerator.LoadAutoGroupedStreamKernel<Index1D, double, ArrayView<Star>, ArrayView<Star>>(GalaxyHelpers.GalaxyKernel);
  }
  public void Update()
  {
    time += GalaxyParams.Dt;
    accelerator.Synchronize();
    bufferA.CopyToCPU(stars);
    Blit();
    for (int i = 0; i < 10; i++)
    {
      galaxyKernel(stars.Length, time, bufferA.View, bufferB.View);
      (bufferB, bufferA) = (bufferA, bufferB);
    }
  }
  public void Blit()
  {
    foreach (var star in stars)
    {
      var pos = new Vector2((float)star.P.X, (float)star.P.Y);
      if (star.Mass == GalaxyParams.StarMass)
        game.Canvas.DrawCircle(pos, radius: 1f, sides: 3, color: Color.White);
      else if (star.Mass == GalaxyParams.StarMass * 1e3f)
        game.Canvas.DrawCircle(pos, radius: 5f, sides: 16, color: Color.Yellow);
      else
        game.Canvas.DrawCircle(pos, radius: 10f, sides: 32, color: Color.Red);
    }
  }
  public void Dispose()
  {
    context.Dispose();
    accelerator.Dispose();
  }
}
