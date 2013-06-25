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

namespace FoundationDB.Tests.Sandbox
{
	using FoundationDB.Client;
	using FoundationDB.Layers.Tuples;
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;

	class Program
	{
		private static int N;
		private static string NATIVE_PATH;
		private static string CLUSTER_FILE;
		private static string DB_NAME;
		private static bool WARNING;

		public static void Main(string[] args)
		{
			N = 16000;
			NATIVE_PATH = null; // set this to the path of the 'bin' folder in your fdb install, like @"C:\Program Files\foundationdb\bin"
			CLUSTER_FILE = null; // set this to the path to your custom fluster file
			DB_NAME = "DB";
			WARNING = true;

			for (int i = 0; i < args.Length; i++)
			{
				if (args[i].StartsWith("-") || args[i].StartsWith("/"))
				{
					switch (args[i].Substring(1).ToLowerInvariant())
					{
						case "db":
						{
							if (i + 1 < args.Length) DB_NAME = args[i++];
							break;
						}
						case "cluster":
						{
							if (i + 1 < args.Length) CLUSTER_FILE = args[i++];
							break;
						}
						case "clear":
						{
							WARNING = false;
							break;
						}
					}
				}
			}

			// Make sure we are on 64 bit
			if (IntPtr.Size == 4)
			{
				Console.Error.WriteLine("This process cannot be run in 32-bit mode !");
				Environment.Exit(-1);
			}

			// Warn the user
			if (WARNING)
			{
				Console.WriteLine("WARNING! WARNING! WARNING!");
				Console.WriteLine("This program will clear all data from your database!");
				Console.WriteLine("Are you sure ? CTRL-C to exit, ENTER to continue");
				Console.ReadLine();
			}

			try
			{
				ExecuteAsync(MainAsync);
			}
			catch (Exception e)
			{
				if (e is AggregateException) e = (e as AggregateException).Flatten().InnerException;
				Console.Error.WriteLine("Oops! something went wrong:");
				Console.Error.WriteLine(e.ToString());
				Environment.ExitCode = -1;
			}
			Console.WriteLine("[PRESS A KEY TO EXIT]");
			Console.ReadKey();
		}

		private static async Task MainAsync()
		{
			// change the path to the native lib if not default
			if (NATIVE_PATH != null) Fdb.Options.SetNativeLibPath(NATIVE_PATH);

			// uncomment this to enable network thread tracing
			// FdbCore.TracePath = Path.Combine(Path.GetTempPath(), "fdb");

			int apiVersion = Fdb.GetMaxApiVersion();
			Console.WriteLine("Max API Version: " + apiVersion);

			try
			{
				Console.WriteLine("Starting network thread...");
				Fdb.Start(); // this will select API version 21			
				Console.WriteLine("> Up and running");

				Console.WriteLine("Connecting to local cluster...");
				using (var cluster = await Fdb.OpenClusterAsync(CLUSTER_FILE))
				{
					Console.WriteLine("> Connected!");

					Console.WriteLine("Opening database 'DB'...");
					using (var db = await cluster.OpenDatabaseAsync(DB_NAME))
					{
						Console.WriteLine("> Connected to db '{0}'", db.Name);

						// get coordinators
						string coordinators = await db.GetCoordinatorsAsync();
						Console.WriteLine("Coordinators: " + coordinators);

						// clear everything
						using (var tr = db.BeginTransaction())
						{
							Console.WriteLine("Clearing database...");
							tr.ClearRange(FdbKey.MinValue, FdbKey.MaxValue);
							await tr.CommitAsync();
							Console.WriteLine("> Database cleared");
						}

						Console.WriteLine("----------");

						await TestSimpleTransactionAsync(db);

						Console.WriteLine("----------");

						await BenchInsertSmallKeysAsync(db, N, 16); // some guid
						//await BenchInsertSmallKeysAsync(db, N, 60 * 4); // one Int32 per minutes, over an hour
						//await BenchInsertSmallKeysAsync(db, N, 512); // small JSON payload
						//await BenchInsertSmallKeysAsync(db, N, 4096); // typical small cunk size
						//await BenchInsertSmallKeysAsync(db, N / 10, 65536); // typical medium chunk size
						//await BenchInsertSmallKeysAsync(db, 1, 100000); // Maximum value size (as of beta 1)

						//// insert keys in parrallel
						//await BenchConcurrentInsert(db, 1, 100, 512);
						//await BenchConcurrentInsert(db, 1, 1000, 512);
						//await BenchConcurrentInsert(db, 1, 10000, 512);

						//await BenchConcurrentInsert(db, 1, N, 16);
						//await BenchConcurrentInsert(db, 2, N, 16);
						//await BenchConcurrentInsert(db, 4, N, 16);
						//await BenchConcurrentInsert(db, 8, N, 16);
						//await BenchConcurrentInsert(db, 16, N, 16);

						//await BenchSerialReadAsync(db, N);

						//await BenchConcurrentReadAsync(db, N);

						//BenchSerialReadBlocking(db, N);

						//await BenchClearAsync(db, N);

						//await BenchUpdateSameKeyLotsOfTimesAsync(db, N);

						//await BenchUpdateLotsOfKeysAsync(db, N);

						await BenchBulkInsertThenBulkReadAsync(db, 100 * 1000, 50, 128);
						await BenchBulkInsertThenBulkReadAsync(db, 100 * 1000, 128, 50);
						//await BenchBulkInsertThenBulkReadAsync(db, 1 * 1000 * 1000, 50, 128);

						//var k1 = FdbKey.Ascii("hello world");
						//Console.WriteLine(k1.ToString());
						//Console.WriteLine(ToHexArray(k1.ToBytes()));

						//var k2 = FdbKey.Pack("hello world", 123);
						//Console.WriteLine(k2.ToString());
						//Console.WriteLine(ToHexArray(k2.ToBytes()));

						//var k2b = FdbTuple.Create("hello world", 123);
						//Console.WriteLine(k2b.ToString());

						//var k3 = FdbKey.Pack(k2b, "yolo");
						//Console.WriteLine(k3.ToString());
						//Console.WriteLine(ToHexArray(k3.ToBytes()));

						//var foos = db.Table("foos");
						//Console.WriteLine(ToHexArray(foos.GetKeyBytes("hello")));
						//Console.WriteLine(ToHexArray(foos.GetKeyBytes(new byte[] { 65, 66, 67 })));
						//Console.WriteLine(ToHexArray(foos.GetKeyBytes(FdbTuple.Create("hello", 123))));

						//string rndid = Guid.NewGuid().ToString();
						//Console.WriteLine(ToHexArray(foos.GetKeyBytes(rndid)));

						//var key = foos.Key(123);
						//Console.WriteLine(key.Count);
						//Console.WriteLine(String.Join(", ", key.ToArray()));

						//using (var trans = db.BeginTransaction())
						//{
						//	foos.Set(trans, rndid, Encoding.UTF8.GetBytes("This is the value of " + rndid));
						//	await trans.CommitAsync();
						//}

						//using (var trans = db.BeginTransaction())
						//{
						//	byte[] value = await foos.GetAsync(trans, rndid);
						//	Console.WriteLine(ToHexArray(value));
						//	value = await trans.GetAsync(FdbTuple.Create("foos", rndid));
						//	Console.WriteLine(ToHexArray(value));
						//}


						//// test range
						//using (var trans = db.BeginTransaction())
						//{
						//	var prefix = FdbTuple.Create("range");
						//	for (int i = 0; i < 100; i++)
						//	{
						//		var k = prefix.Append(i);
						//		Console.WriteLine("Insert: " + ToHexString(k.ToArraySegment()));
						//		trans.Set(k, "value" + i.ToString());
						//	}
						//	await trans.CommitAsync();
						//}

						//using (var trans = db.BeginTransaction())
						//{
						//	Console.WriteLine("Begin: " + ToHexString(FdbKey.Pack("range", 1).ToArraySegment()));
						//	Console.WriteLine("End: " + ToHexString(FdbKey.Pack("range", 7).ToArraySegment()));

						//	var res = await trans.GetRangeAsync(
						//		FdbKeySelector.FirstGreaterOrEqual(FdbKey.Pack("range", 1).ToArraySegment()),
						//		FdbKeySelector.LastLessOrEqual(FdbKey.Pack("range", 7).ToArraySegment()) + 1,
						//		0, 
						//		0,
						//		FDBStreamingMode.WantAll,
						//		0,
						//		false,
						//		false
						//	);

						//	Console.WriteLine("Found " + res.Page.Length + " results");
						//	foreach (var x in res.Page)
						//	{
						//		Console.WriteLine(ToHexString(x.Key) + " : " + Encoding.UTF8.GetString(x.Value.Array, x.Value.Offset, x.Value.Count));
						//	}
						//}

						Console.WriteLine("time to say goodbye...");
					}
				}
			}
			finally
			{
				Console.WriteLine("### DONE ###");
				Fdb.Stop();
			}
		}

		#region Tests...

		private static async Task HelloWorld()
		{

			// Connect to the "DB" database on the local cluster
			using (var db = await Fdb.OpenLocalDatabaseAsync("DB"))
			{

				// Writes some data in to the database
				using (var tr = db.BeginTransaction())
				{
					tr.Set(FdbTuple.Pack("Test", 123), Slice.FromString("Hello World!"));
					tr.Set(FdbTuple.Pack("Test", 456), Slice.FromInt64(DateTime.UtcNow.Ticks));
				}

			}

		}

		private static async Task TestSimpleTransactionAsync(FdbDatabase db)
		{
			Console.WriteLine("Starting new transaction...");

			using (var trans = db.BeginTransaction())
			{
				Console.WriteLine("> Transaction ready");

				Console.WriteLine("Getting read version...");
				var readVersion = await trans.GetReadVersionAsync();
				Console.WriteLine("> Read Version = " + readVersion);

				Console.WriteLine("Getting 'hello'...");
				var result = await trans.GetAsync(FdbKey.Ascii("hello"));
				if (!result.HasValue)
					Console.WriteLine("> hello NOT FOUND");
				else
					Console.WriteLine("> hello = " + result.ToString());

				Console.WriteLine("Setting 'Foo' = 'Bar'");
				trans.Set(FdbKey.Ascii("Foo"), Slice.FromString("Bar"));

				Console.WriteLine("Setting 'TopSecret' = rnd(512)");
				var data = new byte[512];
				new Random(1234).NextBytes(data);
				trans.Set(FdbKey.Ascii("TopSecret"), Slice.Create(data));

				Console.WriteLine("Committing transaction...");
				await trans.CommitAsync();
				//trans.Commit();
				Console.WriteLine("> Committed!");

				Console.WriteLine("Getting comitted version...");
				var writeVersion = trans.GetCommittedVersion();
				Console.WriteLine("> Commited Version = " + writeVersion);
			}
		}

		private static async Task BenchInsertSmallKeysAsync(FdbDatabase db, int N, int size)
		{
			// insert a lot of small key size, in a single transaction
			var rnd = new Random();
			var tmp = new byte[size];

			var table = FdbTuple.Create("Batch");

			var times = new List<TimeSpan>();
			for (int k = 0; k <= 4; k++)
			{
				var sw = Stopwatch.StartNew();
				using (var trans = db.BeginTransaction())
				{
					rnd.NextBytes(tmp);
					for (int i = 0; i < N; i++)
					{
						tmp[0] = (byte)i;
						tmp[1] = (byte)(i >> 8);
						// (Batch, 1) = [......]
						// (Batch, 2) = [......]
						trans.Set(table.Append(k * N + i), Slice.Create(tmp));
					}
					await trans.CommitAsync();
				}
				sw.Stop();
				times.Add(sw.Elapsed);
			}
			var min = times.Min();
			Console.WriteLine("["+ Thread.CurrentThread.ManagedThreadId + "] Took " + min.TotalSeconds.ToString("N3") + " to insert " + N + " " + size + "-bytes items (" + FormatTimeMicro(min.TotalMilliseconds / N) + "/write)");
		}

		private static async Task BenchConcurrentInsert(FdbDatabase db, int k, int N, int size)
		{
			// insert a lot of small key size, in multiple batch running in //
			// k = number of threads
			// N = total number of keys
			// size = value size (bytes)
			// n = keys per batch (N/k)

			int n = N / k;
			// make sure that N is multiple of k
			N = n * k;

			Console.WriteLine("Inserting " + N + " keys in " + k + " batches of " + n + " with " + size + "-bytes values...");

			// store every key under ("Batch", i)
			var table = FdbTuple.Create("Batch");
			// total estimated size of all transactions
			long totalPayloadSize = 0;

			var tasks = new List<Task>();
			var sem = new ManualResetEventSlim();
			for (int j = 0; j < k; j++)
			{
				int offset = j;
				// spin a task for the batch using TaskCreationOptions.LongRunning to make sure it runs in its own thread
				tasks.Add(Task.Factory.StartNew(async () =>
				{
					var rnd = new Random(1234567 * j);
					var tmp = new byte[size];
					rnd.NextBytes(tmp);

					// ("Batch", batch_index, )
					var batch = table.Append(offset);

					// block until all threads are ready
					sem.Wait();

					var x = Stopwatch.StartNew();
					using (var trans = db.BeginTransaction())
					{
						x.Stop();
						Console.WriteLine("> [" + offset + "] got transaction in " + x.Elapsed.TotalMilliseconds.ToString("N3") + " ms");

						// package the keys...
						x.Restart();
						for (int i = 0; i < n; i++)
						{
							// change the value a little bit
							tmp[0] = (byte)i;
							tmp[1] = (byte)(i >> 8);

							// ("Batch", batch_index, i) = [..random..]
							trans.Set(table.Append(i), Slice.Create(tmp));
						}
						x.Stop();
						Console.WriteLine("> [" + offset + "] packaged " + n + " keys (" + trans.Size.ToString("N0") + " bytes) in " + x.Elapsed.TotalMilliseconds.ToString("N3") + " ms");

						// commit the transaction
						x.Restart();
						await trans.CommitAsync();
						x.Stop();
						Console.WriteLine("> [" + offset + "] committed " + n + " keys (" + trans.Size.ToString("N0") + " bytes) in " + x.Elapsed.TotalMilliseconds.ToString("N3") + " ms");

						Interlocked.Add(ref totalPayloadSize, trans.Size);
					}

				}, TaskCreationOptions.LongRunning).Unwrap());
			}
			// give time for threads to be ready
			await Task.Delay(100);

			// start
			var sw = Stopwatch.StartNew();
			sem.Set();

			// wait for total completion
			await Task.WhenAll(tasks);
			sw.Stop();
			Console.WriteLine("* Total: " + sw.Elapsed.TotalMilliseconds.ToString("N1") + "ms, " + FormatTimeMicro(sw.Elapsed.TotalMilliseconds / N) + "/write, " + (totalPayloadSize / (sw.Elapsed.TotalSeconds * 1024)).ToString("N1") + "kB/sec");
			Console.WriteLine();
		}

		private static async Task BenchSerialReadAsync(FdbDatabase db, int N)
		{
			// read a lot of small keys, one by one

			var sw = Stopwatch.StartNew();
			using (var trans = db.BeginTransaction())
			{
				for (int i = 0; i < N; i++)
				{
					var result = await trans.GetAsync(FdbKey.Ascii("hello" + i));
				}
			}
			sw.Stop();
			Console.WriteLine("Took " + sw.Elapsed + " to read " + N + " items (" + FormatTimeMicro(sw.Elapsed.TotalMilliseconds / N) + "/read)");
		}

		private static async Task BenchConcurrentReadAsync(FdbDatabase db, int N)
		{
			// read a lot of small keys, concurrently

			var sw = Stopwatch.StartNew();
			using (var trans = db.BeginTransaction())
			{
				var results = await Task.WhenAll(Enumerable
					.Range(0, N)
					.Select((i) => trans.GetAsync(FdbKey.Ascii("hello" + i)))
				);
			}
			sw.Stop();
			Console.WriteLine("Took " + sw.Elapsed + " to read " + N + " items (" + FormatTimeMicro(sw.Elapsed.TotalMilliseconds / N) + "/read)");

			var keys = Enumerable.Range(0, N).Select(i => FdbKey.Ascii("hello" + i)).ToArray();

			sw = Stopwatch.StartNew();
			using (var trans = db.BeginTransaction())
			{
				var results = await trans.GetBatchAsync(keys);
			}
			sw.Stop();
			Console.WriteLine("Took " + sw.Elapsed + " to read " + keys.Length + " items (" + FormatTimeMicro(sw.Elapsed.TotalMilliseconds / keys.Length) + "/read)");
		}

		private static async Task BenchClearAsync(FdbDatabase db, int N)
		{
			// clear a lot of small keys, in a single transaction

			var sw = Stopwatch.StartNew();
			using (var trans = db.BeginTransaction())
			{
				for (int i = 0; i < N; i++)
				{
					trans.Clear(FdbKey.Ascii("hello" + i));
				}

				await trans.CommitAsync();
			}
			sw.Stop();
			Console.WriteLine("Took " + sw.Elapsed + " to clear " + N + " items (" + FormatTimeMicro(sw.Elapsed.TotalMilliseconds / N) + "/write)");
		}

		private static async Task BenchUpdateSameKeyLotsOfTimesAsync(FdbDatabase db, int N)
		{
			// continuously update same key by adding a little bit more

			var list = new byte[N];
			var update = Stopwatch.StartNew();
			for (int i = 0; i < N; i++)
			{
				list[i] = (byte)i;
				using (var trans = db.BeginTransaction())
				{
					trans.Set(FdbKey.Ascii("list"), Slice.Create(list));
					await trans.CommitAsync();
				}
			}
			update.Stop();

			Console.WriteLine("Took " + update.Elapsed + " to fill a byte[" + N + "] one by one (" + FormatTimeMicro(update.Elapsed.TotalMilliseconds / N) + "/write)");
		}

		private static async Task BenchUpdateLotsOfKeysAsync(FdbDatabase db, int N)
		{
			// continuously update same key by adding a little bit more

			var keys = Enumerable.Range(0, N).Select(x => FdbKey.Ascii("list" + x.ToString())).ToArray();

			Console.WriteLine("> creating " + N + " half filled keys");
			var segment = new byte[60];

			for (int i = 0; i < (segment.Length >> 1); i++) segment[i] = (byte)(i >> 2);
			using (var trans = db.BeginTransaction())
			{
				for (int i = 0; i < N; i++)
				{
					trans.Set(keys[i], Slice.Create(segment));
				}
				await trans.CommitAsync();
			}

			Console.WriteLine("Changing one byte in each of the " + N + " keys...");
			var sw = Stopwatch.StartNew();
			using (var trans = db.BeginTransaction())
			{
				Console.WriteLine("READ");
				// get all the lists
				var data = await trans.GetBatchAsync(keys);

				// change them
				Console.WriteLine("CHANGE");
				for (int i = 0; i < data.Count; i++)
				{
					var list = data[i].Value;
					list.Array[list.Offset + (list.Count >> 1) + 1] = (byte)i;
					trans.Set(data[i].Key, list);
				}

				Console.WriteLine("COMMIT");
				await trans.CommitAsync();
			}
			sw.Stop();

			Console.WriteLine("Took " + sw.Elapsed + " to change a byte in " + N + " lists (" + FormatTimeMicro(sw.Elapsed.TotalMilliseconds / N) + " /write)");

		}

		private static async Task BenchBulkInsertThenBulkReadAsync(FdbDatabase db, int N, int K, int B, bool instrumented = false)
		{
			// test that we can bulk write / bulk read

			var timings = instrumented ? new List<KeyValuePair<double, double>>() : null;

			// put test values inside a namespace
			var tuple = FdbTuple.Create("BulkInsert");

			// cleanup everything
			using (var tr = db.BeginTransaction())
			{
				tr.ClearRange(tuple);
				await tr.CommitAsync();
			}

			// insert all values (batched)
			Console.WriteLine("Inserting " + N.ToString("N0") + " keys: ");
			var insert = Stopwatch.StartNew();
			int batches = 0;
			long bytes = 0;

			var start = Stopwatch.StartNew();

			var tasks = new List<Task>();
			foreach (var worker in FdbKey.Batched(0, N, K, B))
			{
				//hack
				tasks.Add(Task.Run(async () =>
				{
					foreach (var chunk in worker)
					{
						using (var tr = db.BeginTransaction())
						{
							int z = 0;
							foreach (int i in Enumerable.Range(chunk.Key, chunk.Value))
							{
								tr.Set(tuple.Append(i), Slice.Create(new byte[256]));
								z++;
							}

							//Console.Write("#");
							//Console.WriteLine("  Commiting batch (" + tr.Size.ToString("N0") + " bytes) " + z + " keys");
							var localStart = start.Elapsed.TotalSeconds;
							await tr.CommitAsync();
							var localDuration = start.Elapsed.TotalSeconds - localStart;
							if (instrumented)
							{
								lock (timings) { timings.Add(new KeyValuePair<double, double>(localStart, localDuration)); }
							}
							Interlocked.Increment(ref batches);
							Interlocked.Add(ref bytes, tr.Size);
						}

					}
				}));

			}
			await Task.WhenAll(tasks);

			insert.Stop();
			Console.WriteLine("Committed " + batches + " batches in " + insert.Elapsed.TotalMilliseconds.ToString("N1") + " ms (" + (insert.Elapsed.TotalMilliseconds / batches).ToString("N2") + " ms / batch, " + (insert.Elapsed.TotalMilliseconds * 1000 / N).ToString("N3") + " µs / item");
			Console.WriteLine("Throughput " + (bytes / (1024.0 * 1024.0 * insert.Elapsed.TotalSeconds)).ToString("N3") + " MB/s");

			if (instrumented)
			{
				var sb = new StringBuilder();
				foreach (var kvp in timings)
				{
					sb.Append(kvp.Key.ToString()).Append(';').Append((kvp.Key + kvp.Value).ToString()).Append(';').Append(kvp.Value.ToString()).AppendLine();
				}
				System.IO.File.WriteAllText(@"c:\temp\fdb\timings_" + N + "_" + K + "_" + B + ".csv", sb.ToString());
			}

			// Read values

			using (var tr = db.BeginTransaction())
			{
				var res = tr.GetRange(tuple.Append(0), tuple.Append(N));

				Console.WriteLine("Reading all keys...");
				var range = Stopwatch.StartNew();
				var items = await res.ToListAsync();
				range.Stop();
				Console.WriteLine("Took " + range.Elapsed.TotalMilliseconds.ToString("N1") + " ms to get " + items.Count.ToString("N0") + " results");
			}
		}

		#endregion

		#region Helpers...

		private static void ExecuteAsync(Func<Task> code)
		{
			// poor man's async main loop
			Task.Run(code).GetAwaiter().GetResult();
		}

		private static string FormatTimeMicro(double ms)
		{
			return (1000 * ms).ToString("N1") + "µs";
		}

		#endregion

	}
}