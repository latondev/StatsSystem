using System;

namespace GameSystems.Stats
{
	internal interface IOperator<T>
	{
		T Add(T a, T b);
		T Subtract(T a, T b);
		T Multiply(T a, T b);
		T Divide(T a, T b);
		T Zero();
		T One();
	}

	internal static class Operator<T>
	{
		private static IOperator<T> _instance;

		public static IOperator<T> Instance
		{
			get
			{
				if (_instance == null)
				{
					if (typeof(T) == typeof(float))
						_instance = (IOperator<T>)(object)new OpFloat();
					else if (typeof(T) == typeof(int))
						_instance = (IOperator<T>)(object)new OpInt();
					else if (typeof(T) == typeof(double))
						_instance = (IOperator<T>)(object)new OpDouble();
					else
						throw new NotSupportedException($"Type {typeof(T)} is not supported");
				}
				return _instance;
			}
		}
	}

	internal struct OpFloat : IOperator<float>
	{
		public float Add(float a, float b) => a + b;
		public float Subtract(float a, float b) => a - b;
		public float Multiply(float a, float b) => a * b;
		public float Divide(float a, float b) => a / b;
		public float Zero() => 0f;
		public float One() => 1f;
	}

	internal struct OpInt : IOperator<int>
	{
		public int Add(int a, int b) => a + b;
		public int Subtract(int a, int b) => a - b;
		public int Multiply(int a, int b) => a * b;
		public int Divide(int a, int b) => a / b;
		public int Zero() => 0;
		public int One() => 1;
	}

	internal struct OpDouble : IOperator<double>
	{
		public double Add(double a, double b) => a + b;
		public double Subtract(double a, double b) => a - b;
		public double Multiply(double a, double b) => a * b;
		public double Divide(double a, double b) => a / b;
		public double Zero() => 0.0;
		public double One() => 1.0;
	}
}
