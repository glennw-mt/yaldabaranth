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
  MemoryBuffer1D<Star, Stride1D.Dense> bufferA;
  MemoryBuffer1D<Star, Stride1D.Dense> bufferB;
  readonly Action<Index1D, double, ArrayView<Star>, ArrayView<Star>> nBodyKernel;
  private readonly Star[] stars = [];
  private readonly Star[] blackHoles = [];
  private readonly double nStars = 1e4;
  private static readonly float blackHoleMass = 1e6f;
  private static readonly float starMass = 1f;
  private static readonly float width = 1000f;
  private static readonly float height = 1000f;
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
    double softening = 0.1f;
    for (int i = 0; i < current.Length; i++)
    {
      if (i == index) continue;
      Star other = current[i];
      if (other.mass == starMass) continue;
      Float64x2 r = other.P - me.P;
      if (r.X > width / 2) r -= new Float64x2(width, 0);
      else if (r.X < -width / 2) r += new Float64x2(width, 0);

      if (r.Y > height / 2) r -= new Float64x2(0, height);
      else if (r.Y < -height / 2) r += new Float64x2(0, height);
      double rSquared = r.X * r.X + r.Y * r.Y + softening;
      double rAbs = XMath.Sqrt(rSquared);
      double forceMag = me.mass * other.mass / rSquared;
      totalForce += new Float64x2(r.X / rAbs * forceMag, r.Y / rAbs * forceMag);
    }
    Float64x2 acceleration = new(totalForce.X / me.mass, totalForce.Y / me.mass);
    double newX = me.P.X + me.V.X * dt;
    double newY = me.P.Y + me.V.Y * dt;
    if (newX < 0) newX = width;
    if (newX > width) newX = 0;
    if (newY < 0) newY = height;
    if (newY > height) newY = 0;
    next[index] = new Star(
        new Float64x2(newX, newY),
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
    Console.WriteLine("Generating initial stars.");
    var starList = new List<Star>();
    var blackHoleList = new List<Star>();
    var noiseGenerator = new FastNoiseLite(rnd.Next());
    var blackHole = new Star(new Float64x2(width / 2, height / 2), new Float64x2(0, 0), blackHoleMass);
    blackHoleList.Add(blackHole);
    float scaleLength = width / 8;
    while (starList.Count < nStars)
    {
      var parentPos = blackHoleList[rnd.Next(blackHoleList.Count)].P;
      double u = rnd.NextDouble();
      double r = -scaleLength * Math.Log(1.0 - u) + 5f;
      double theta = rnd.NextDouble() * 2.0 * Math.PI;
      double x = parentPos.X + r * Math.Cos(theta) * 1.1f;
      double y = parentPos.Y + r * Math.Sin(theta) * 0.9f;
      x = (x % width + width) % width;
      y = (y % height + height) % height;
      Float64x2 finalPos = new(x, y);
      var star = new Star(finalPos, new Float64x2(0, 0), starMass);
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
        if (r.X > width / 2) r -= new Float64x2(width, 0);
        else if (r.X < -width / 2) r += new Float64x2(width, 0);

        if (r.Y > height / 2) r -= new Float64x2(0, height);
        else if (r.Y < -height / 2) r += new Float64x2(0, height);
        double sDist = r.X * r.X + r.Y * r.Y;
        if (sDist < minSDist)
        {
          minSDist = sDist;
          bestR = r;
        }
      }
      double dist = Math.Sqrt(minSDist);
      double orbitalSpeed = Math.Sqrt(blackHoleMass / dist);
      Float64x2 unitPerp = new(-bestR.Y / dist, bestR.X / dist);
      s.V = new Float64x2(unitPerp.X * orbitalSpeed, unitPerp.Y * orbitalSpeed);
      stars[i] = s;
    }
    // stars = [.. starList, .. blackHoleList];
    bufferA = accelerator.Allocate1D<Star>(stars.Length);
    bufferB = accelerator.Allocate1D<Star>(stars.Length);
    bufferA.CopyFromCPU(stars);
    nBodyKernel = accelerator.LoadAutoGroupedStreamKernel<Index1D, double, ArrayView<Star>, ArrayView<Star>>(NBodyKernelDef);
  }
  public void Update()
  {
    accelerator.Synchronize();
    bufferA.CopyToCPU(stars);
    Blit();
    double deltaTime = 0.0001d;
    for (int i = 0; i < 10; i++)
    {
      nBodyKernel(stars.Length, deltaTime, bufferA.View, bufferB.View);
      (bufferB, bufferA) = (bufferA, bufferB);

    }
  }
  public void Blit()
  {
    foreach (var star in stars)
    {
      var pos = star.P;
      if (star.mass == starMass)
        game.Canvas.DrawCircle(new Vector2((float)pos.X, (float)pos.Y), radius: 1f, sides: 2, color: Color.White);
      else if (star.mass == starMass * 1e3f)
        game.Canvas.DrawCircle(new Vector2((float)pos.X, (float)pos.Y), radius: 5f, sides: 3, color: Color.Yellow);
      else
        game.Canvas.DrawCircle(new Vector2((float)pos.X, (float)pos.Y), radius: 10f, sides: 4, color: Color.Red);
      game.Canvas.DrawRectangle(Vector2.Zero, new Vector2(width, height), Color.Red);
    }
  }
  public void Dispose()
  {
    context.Dispose();
    accelerator.Dispose();
  }
}
