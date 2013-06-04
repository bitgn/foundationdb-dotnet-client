﻿#region BSD Licence
/* Copyright (c) 2013, Doxense SARL
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:
	* Redistributions of source code must retain the above copyright
	  notice, this list of conditions and the following disclaimer.
	* Redistributions in binary form must reproduce the above copyright
	  notice, this list of conditions and the following disclaimer in the
	  documentation and/or other materials provided with the distribution.
	* Neither the name of the <organization> nor the
	  names of its contributors may be used to endorse or promote products
	  derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion

using FoundationDb.Client.Converters;
using FoundationDb.Client.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace FoundationDb.Client.Tuples
{

	/// <summary>Tuple that can hold three items</summary>
	/// <typeparam name="T1">Type of the first item</typeparam>
	/// <typeparam name="T2">Type of the second item</typeparam>
	/// <typeparam name="T3">Type of the third item</typeparam>
	[DebuggerDisplay("({Item1}, {Item2}, {Item3})")]
	public struct FdbTuple<T1, T2, T3> : IFdbTuple
	{
		public readonly T1 Item1;
		public readonly T2 Item2;
		public readonly T3 Item3;

		public FdbTuple(T1 item1, T2 item2, T3 item3)
		{
			this.Item1 = item1;
			this.Item2 = item2;
			this.Item3 = item3;
		}

		public int Count { get { return 3; } }

		public object this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: case -3: return this.Item1;
					case 1: case -2: return this.Item2;
					case 2: case -1: return this.Item3;
					default: throw new IndexOutOfRangeException();
				}
			}
		}

		public R Get<R>(int index)
		{
			switch(index)
			{
					case 0: case -3: return FdbConverters.Convert<T1, R>(this.Item1);
					case 1: case -2: return FdbConverters.Convert<T2, R>(this.Item2);
					case 2: case -1: return FdbConverters.Convert<T3, R>(this.Item3);
					default: throw new IndexOutOfRangeException();
			}
		}

		public void PackTo(FdbBufferWriter writer)
		{
			FdbTuplePacker<T1>.SerializeTo(writer, this.Item1);
			FdbTuplePacker<T2>.SerializeTo(writer, this.Item2);
			FdbTuplePacker<T3>.SerializeTo(writer, this.Item3);
		}

		IFdbTuple IFdbTuple.Append<T4>(T4 value)
		{
			return this.Append<T4>(value);
		}

		public FdbTuple<T1, T2, T3, T4> Append<T4>(T4 value)
		{
			return new FdbTuple<T1, T2, T3, T4>(this.Item1, this.Item2, this.Item3, value);
		}

		public void CopyTo(object[] array, int offset)
		{
			array[offset] = this.Item1;
			array[offset + 1] = this.Item2;
			array[offset + 2] = this.Item3;
		}

		public IEnumerator<object> GetEnumerator()
		{
			yield return this.Item1;
			yield return this.Item2;
			yield return this.Item3;
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public Slice ToSlice()
		{
			var writer = new FdbBufferWriter();
			PackTo(writer);
			return writer.ToSlice();
		}

		public override string ToString()
		{
			return new StringBuilder().Append('(').Append(this.Item1).Append(", ").Append(this.Item2).Append(", ").Append(this.Item3).Append(')').ToString();
		}

		public override int GetHashCode()
		{
			int h;
			h = this.Item1 != null ? this.Item1.GetHashCode() : -1;
			h ^= this.Item2 != null ? this.Item2.GetHashCode() : -1;
			h ^= this.Item3 != null ? this.Item3.GetHashCode() : -1;
			return h;
		}

	}

}