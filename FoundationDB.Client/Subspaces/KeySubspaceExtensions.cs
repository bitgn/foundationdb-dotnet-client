﻿#region BSD Licence
/* Copyright (c) 2013-2018, Doxense SAS
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:
	* Redistributions of source code must retain the above copyright
	  notice, this list of conditions and the following disclaimer.
	* Redistributions in binary form must reproduce the above copyright
	  notice, this list of conditions and the following disclaimer in the
	  documentation and/or other materials provided with the distribution.
	* Neither the name of Doxense nor the
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

namespace FoundationDB.Client
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using System.Threading.Tasks;
	using Doxense.Collections.Tuples;
	using Doxense.Diagnostics.Contracts;
	using Doxense.Serialization.Encoders;
	using JetBrains.Annotations;

	/// <summary>Extensions methods and helpers to work with Key Subspaces</summary>
	public static class KeySubspaceExtensions
	{

		#region Encodings...

		/// <summary>Return a version of this subspace, which uses a different type system to produces the keys and values</summary>
		/// <param name="subspace">Instance of a generic subspace</param>
		/// <param name="encoding">If non-null, uses this specific instance of the TypeSystem. If null, uses the default instance for this particular TypeSystem</param>
		/// <returns>Subspace equivalent to <paramref name="subspace"/>, but augmented with a specific TypeSystem</returns>
		[Pure, NotNull]
		public static IDynamicKeySubspace AsDynamic([NotNull] this IKeySubspace subspace, IKeyEncoding encoding = null)
		{
			Contract.NotNull(subspace, nameof(subspace));
			return new DynamicKeySubspace(subspace.GetPrefix(), encoding ?? TuPack.Encoding);
		}

		/// <summary>Return a version of this subspace, which uses a different type system to produces the keys and values</summary>
		/// <param name="subspace">Instance of a generic subspace to extend</param>
		/// <param name="encoding">Encoding by the keys of this subspace. If not specified, the <see cref="TuPack">Tuple Encoding</see> will be used to generate an encoder.</param>
		/// <returns>Subspace equivalent to <paramref name="subspace"/>, but augmented with a specific TypeSystem</returns>
		[Pure, NotNull]
		public static ITypedKeySubspace<T> AsTyped<T>([NotNull] this IKeySubspace subspace, [CanBeNull] IKeyEncoding encoding = null)
		{
			Contract.NotNull(subspace, nameof(subspace));
			return new TypedKeySubspace<T>(subspace.GetPrefix(), (encoding ?? TuPack.Encoding).GetKeyEncoder<T>());
		}

		/// <summary>Return a version of this subspace, which uses a different type system to produces the keys and values</summary>
		/// <param name="subspace">Instance of a generic subspace to extend</param>
		/// <param name="encoding">Encoding used by the keys of this subspace. If not specified, the <see cref="TuPack">Tuple Encoding</see> will be used to generate an encoder.</param>
		/// <returns>Subspace equivalent to <paramref name="subspace"/>, but augmented with a specific TypeSystem</returns>
		[Pure, NotNull]
		public static ITypedKeySubspace<T1, T2> AsTyped<T1, T2>([NotNull] this IKeySubspace subspace, [CanBeNull] IKeyEncoding encoding = null)
		{
			Contract.NotNull(subspace, nameof(subspace));
			return new TypedKeySubspace<T1, T2>(subspace.GetPrefix(), (encoding ?? TuPack.Encoding).GetKeyEncoder<T1, T2>());
		}

		/// <summary>Return a version of this subspace, which uses a different type system to produces the keys and values</summary>
		/// <param name="subspace">Instance of a generic subspace to extend</param>
		/// <param name="encoding">Encoding used by the keys of this subspace. If not specified, the <see cref="TuPack">Tuple Encoding</see> will be used to generate an encoder.</param>
		/// <returns>Subspace equivalent to <paramref name="subspace"/>, but augmented with a specific TypeSystem</returns>
		[Pure, NotNull]
		public static ITypedKeySubspace<T1, T2, T3> AsTyped<T1, T2, T3>([NotNull] this IKeySubspace subspace, [CanBeNull] IKeyEncoding encoding = null)
		{
			Contract.NotNull(subspace, nameof(subspace));
			return new TypedKeySubspace<T1, T2, T3>(subspace.GetPrefix(), (encoding ?? TuPack.Encoding).GetKeyEncoder<T1, T2, T3>());
		}

		/// <summary>Return a version of this subspace, which uses a different type system to produces the keys and values</summary>
		/// <param name="subspace">Instance of a generic subspace</param>
		/// <param name="encoding">Encoding used by the keys of this namespace. If not specified, the <see cref="TuPack">Tuple Encoding</see> will be used to generate an encoder.</param>
		/// <returns>Subspace equivalent to <paramref name="subspace"/>, but augmented with a specific TypeSystem</returns>
		[Pure, NotNull]
		public static ITypedKeySubspace<T1, T2, T3, T4> AsTyped<T1, T2, T3, T4>([NotNull] this IKeySubspace subspace, [CanBeNull] IKeyEncoding encoding = null)
		{
			Contract.NotNull(subspace, nameof(subspace));
			return new TypedKeySubspace<T1, T2, T3, T4>(subspace.GetPrefix(), (encoding ?? TuPack.Encoding).GetKeyEncoder<T1, T2, T3, T4>());
		}


		#endregion

		#region Encoders...

		/// <summary>Return a version of this subspace, which uses a different type system to produces the keys and values</summary>
		/// <param name="subspace">Instance of a generic subspace to extend</param>
		/// <param name="encoder">Custom key encoder</param>
		/// <returns>Subspace equivalent to <paramref name="subspace"/>, but augmented with a specific TypeSystem</returns>
		[Pure, NotNull]
		public static IDynamicKeySubspace UsingEncoder([NotNull] this IKeySubspace subspace, [NotNull] IDynamicKeyEncoder encoder)
		{
			Contract.NotNull(subspace, nameof(subspace));
			Contract.NotNull(encoder, nameof(encoder));
			return new DynamicKeySubspace(subspace.GetPrefix(), encoder.Encoding);
		}

		/// <summary>Return a version of this subspace, which uses a different type system to produces the keys and values</summary>
		/// <param name="subspace">Instance of a generic subspace to extend</param>
		/// <param name="encoder">Custom key encoder</param>
		/// <returns>Subspace equivalent to <paramref name="subspace"/>, but augmented with a specific TypeSystem</returns>
		[Pure, NotNull]
		public static ITypedKeySubspace<T> UsingEncoder<T>([NotNull] this IKeySubspace subspace, [NotNull] IKeyEncoder<T> encoder)
		{
			Contract.NotNull(subspace, nameof(subspace));
			Contract.NotNull(encoder, nameof(encoder));
			return new TypedKeySubspace<T>(subspace.GetPrefix(), encoder);
		}

		/// <summary>Return a version of this subspace, which uses a different type system to produces the keys and values</summary>
		/// <param name="subspace">Instance of a generic subspace to extend</param>
		/// <param name="encoder">Custom key encoder</param>
		/// <returns>Subspace equivalent to <paramref name="subspace"/>, but augmented with a specific TypeSystem</returns>
		[Pure, NotNull]
		public static ITypedKeySubspace<T1, T2> UsingEncoder<T1, T2>([NotNull] this IKeySubspace subspace, [NotNull] ICompositeKeyEncoder<T1, T2> encoder)
		{
			Contract.NotNull(subspace, nameof(subspace));
			Contract.NotNull(encoder, nameof(encoder));
			return new TypedKeySubspace<T1, T2>(subspace.GetPrefix(), encoder);
		}

		/// <summary>Return a version of this subspace, which uses a different type system to produces the keys and values</summary>
		/// <param name="subspace">Instance of a generic subspace to extend</param>
		/// <param name="encoder">Custom key encoder</param>
		/// <returns>Subspace equivalent to <paramref name="subspace"/>, but augmented with a specific TypeSystem</returns>
		[Pure, NotNull]
		public static ITypedKeySubspace<T1, T2, T3> UsingEncoder<T1, T2, T3>([NotNull] this IKeySubspace subspace, [NotNull] ICompositeKeyEncoder<T1, T2, T3> encoder)
		{
			Contract.NotNull(subspace, nameof(subspace));
			Contract.NotNull(encoder, nameof(encoder));
			return new TypedKeySubspace<T1, T2, T3>(subspace.GetPrefix(), encoder);
		}

		/// <summary>Return a version of this subspace, which uses a different type system to produces the keys and values</summary>
		/// <param name="subspace">Instance of a generic subspace</param>
		/// <param name="encoder">Encoder used to serialize the keys of this namespace.</param>
		/// <returns>Subspace equivalent to <paramref name="subspace"/>, but augmented with a specific TypeSystem</returns>
		[Pure, NotNull]
		public static ITypedKeySubspace<T1, T2, T3, T4> UsingEncoder<T1, T2, T3, T4>([NotNull] this IKeySubspace subspace, [NotNull] ICompositeKeyEncoder<T1, T2, T3, T4> encoder)
		{
			Contract.NotNull(subspace, nameof(subspace));
			Contract.NotNull(encoder, nameof(encoder));
			return new TypedKeySubspace<T1, T2, T3, T4>(subspace.GetPrefix(), encoder);
		}

		#endregion

		#region Copy...

		/// <summary>Create a new copy of a subspace's prefix</summary>
		[Pure]
		internal static Slice StealPrefix([NotNull] IKeySubspace subspace)
		{
			//note: we can workaround the 'security' in top directory partition by accessing their key prefix without triggering an exception!
			return subspace is KeySubspace ks
				? ks.GetPrefixUnsafe().Memoize()
				: subspace.GetPrefix().Memoize();
		}

		/// <summary>Create a copy of a generic subspace, sharing the same binary prefix</summary>
		[Pure, NotNull]
		public static KeySubspace Copy([NotNull] this IKeySubspace subspace)
		{
			Contract.NotNull(subspace, nameof(subspace));

			var prefix = StealPrefix(subspace);

			if (subspace is IDynamicKeySubspace dyn)
			{ // reuse the encoding of the original
				return new DynamicKeySubspace(prefix, dyn.Encoding);
			}

			// no encoding
			return new KeySubspace(prefix);
		}

		/// <summary>Create a copy of a generic subspace, sharing the same binary prefix</summary>
		[Pure, NotNull]
		public static DynamicKeySubspace Copy([NotNull] this IKeySubspace subspace, IKeyEncoding encoding)
		{
			Contract.NotNull(subspace, nameof(subspace));
			Contract.NotNull(encoding, nameof(encoding));
			return new DynamicKeySubspace(StealPrefix(subspace), encoding);
		}

		/// <summary>Create a copy of a generic subspace, sharing the same binary prefix</summary>
		[Pure, NotNull]
		public static DynamicKeySubspace Copy([NotNull] this IKeySubspace subspace, IDynamicKeyEncoder encoder)
		{
			Contract.NotNull(subspace, nameof(subspace));
			Contract.NotNull(encoder, nameof(encoder));
			return new DynamicKeySubspace(StealPrefix(subspace), encoder);
		}

		/// <summary>Create a copy of a dynamic subspace, sharing the same binary prefix and encoder</summary>
		[Pure, NotNull]
		public static DynamicKeySubspace Copy([NotNull] this IDynamicKeySubspace subspace)
		{
			Contract.NotNull(subspace, nameof(subspace));
			return new DynamicKeySubspace(StealPrefix(subspace), subspace.Encoding);
		}

		/// <summary>Create a copy of a typed subspace, sharing the same binary prefix and encoder</summary>
		[Pure, NotNull]
		public static TypedKeySubspace<T1> Copy<T1>([NotNull] this ITypedKeySubspace<T1> subspace)
		{
			Contract.NotNull(subspace, nameof(subspace));
			return new TypedKeySubspace<T1>(StealPrefix(subspace), subspace.KeyEncoder);
		}

		/// <summary>Create a copy of a typed subspace, sharing the same binary prefix and encoder</summary>
		[Pure, NotNull]
		public static TypedKeySubspace<T1, T2> Copy<T1, T2>([NotNull] this ITypedKeySubspace<T1, T2> subspace)
		{
			Contract.NotNull(subspace, nameof(subspace));
			return new TypedKeySubspace<T1, T2>(StealPrefix(subspace), subspace.KeyEncoder);
		}

		/// <summary>Create a copy of a typed subspace, sharing the same binary prefix and encoder</summary>
		[Pure, NotNull]
		public static TypedKeySubspace<T1, T2, T3> Copy<T1, T2, T3>([NotNull] this ITypedKeySubspace<T1, T2, T3> subspace)
		{
			Contract.NotNull(subspace, nameof(subspace));
			return new TypedKeySubspace<T1, T2, T3>(StealPrefix(subspace), subspace.KeyEncoder);
		}

		/// <summary>Create a copy of a typed subspace, sharing the same binary prefix and encoder</summary>
		[Pure, NotNull]
		public static TypedKeySubspace<T1, T2, T3, T4> Copy<T1, T2, T3, T4>([NotNull] this ITypedKeySubspace<T1, T2, T3, T4> subspace)
		{
			Contract.NotNull(subspace, nameof(subspace));
			return new TypedKeySubspace<T1, T2, T3, T4>(StealPrefix(subspace), subspace.KeyEncoder);
		}

		#endregion

		/// <summary>Clear the entire content of a subspace</summary>
		public static void ClearRange(this IFdbTransaction trans, [NotNull] IKeySubspace subspace)
		{
			Contract.Requires(trans != null && subspace != null);

			//BUGBUG: should we call subspace.ToRange() ?
			trans.ClearRange(subspace.ToRange());
		}

		/// <summary>Clear the entire content of a subspace</summary>
		public static Task ClearRangeAsync(this IFdbRetryable db, [NotNull] IKeySubspace subspace, CancellationToken ct)
		{
			Contract.NotNull(db, nameof(db));
			Contract.NotNull(subspace, nameof(subspace));

			return db.WriteAsync((tr) => ClearRange(tr, subspace), ct);
		}

		/// <summary>Returns all the keys inside of a subspace</summary>
		[Pure, NotNull]
		public static FdbRangeQuery<KeyValuePair<Slice, Slice>> GetRangeStartsWith(this IFdbReadOnlyTransaction trans, [NotNull] IKeySubspace subspace, FdbRangeOptions options = null)
		{
			//REVIEW: should we remove this method?
			Contract.Requires(trans != null && subspace != null);

			return trans.GetRange(subspace.ToRange(), options);
		}

	}
}
