// http://jacksondunstan.com/articles/3349

using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit)]
public struct Union<TLeft, TRight>
{
	[FieldOffset(0)] public TLeft Left;
	[FieldOffset(0)] public TRight Right;
}

public struct Either<TLeft, TRight>
{
	private bool isLeft;
	private Union<TLeft, TRight> union;

	public Either(TLeft left)
	{
		isLeft = true;
		union.Right = default(TRight);
		union.Left = left;
	}

	public Either(TRight right)
	{
		isLeft = false;
		union.Left = default(TLeft);
		union.Right = right;
	}

	public TLeft Left
	{
		get
		{
			if (isLeft == false)
			{
				throw new Exception("Either doesn't hold Left");
			}
			return union.Left;
		}
		set
		{
			union.Left = value;
			isLeft = true;
		}
	}

	public TRight Right
	{
		get
		{
			if (isLeft)
			{
				throw new Exception("Either doesn't hold Right");
			}
			return union.Right;
		}
		set
		{
			union.Right = value;
			isLeft = false;
		}
	}

	public bool IsLeft
	{
		get { return isLeft; }
	}
}

public static class EitherExtensions
{
	public static TResult Match<TLeft, TRight, TResult>(
		this Either<TLeft, TRight> either,
		Func<TLeft, TResult> leftMatcher,
		Func<TRight, TResult> rightMatcher
	)
	{
		if (either.IsLeft)
		{
			return leftMatcher(either.Left);
		}
		else
		{
			return rightMatcher(either.Right);
		}
	}
}

public static class EitherUtils
{
	public static Func<Either<TA, TC>, Either<TB, TC>>
		Bind<TA, TB, TC>(
			Func<TA, Either<TB, TC>> func
		)
	{
		return e => e.Match(
			a => func(a),
			c => new Either<TB, TC>(c)
		);
	}

	public static Func<T, T> ReturnParam<T>(Action<T> func)
	{
		return arg => {
			func(arg);
			return arg;
		};
	}

	public static Func<TA, Either<TA, TB>> ReturnEitherLeft<TA, TB>(Func<TA, TA> func)
	{
		return arg => new Either<TA, TB>(func(arg));
	}

	public static Func<TA, TB>
		Combine<TA, TB>(
			Func<TA, TB> firstFunc,
			params Func<TB, TB>[] moreFuncs
		)
	{
		return arg =>
		{
			var ret = firstFunc(arg);
			foreach (var func in moreFuncs)
			{
				ret = func(ret);
			}
			return ret;
		};
	}

	public static Func<TA, Either<TB, TC>> Identity<TA, TB, TC>(Func<TA, Either<TB, TC>> func)
	{
		return func;
	}
}