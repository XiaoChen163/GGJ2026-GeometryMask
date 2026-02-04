using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Shape
{
    Circle,
    Square,
    Triangle
}

public class ShapeWithMask
{
    private Shape shape;
    private Shape mask;
    public bool hasMask;

    public ShapeWithMask()
    {
        hasMask = false;
    }

    public Shape GetMaskedShape()
    {
        if (hasMask) return mask;
        return shape;
    }

    public Shape GetTrueShape()
    {
        return shape;
    }

    public void SetMask(Shape mask)
    {
        this.mask = mask;
        hasMask = true;
    }

    public void RemoveMask()
    {
        hasMask = false;
    }

    public void RandomInit()
    {
        shape = (Shape)Random.Range(0, 3);
        hasMask = false;
    }
}
