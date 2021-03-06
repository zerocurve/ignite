﻿/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Apache.Ignite.Core.Tests.Cache
{
    using System;
    using System.Threading;
    using Apache.Ignite.Core.Cache;
    using Apache.Ignite.Core.Cache.Configuration;
    using Apache.Ignite.Core.Discovery.Tcp;
    using Apache.Ignite.Core.Impl;
    using Apache.Ignite.Core.Impl.Cache;
    using NUnit.Framework;

    /// <summary>
    /// Tests cache metrics propagation.
    /// </summary>
    public class CacheMetricsTest
    {
        /** */
        private const string SecondGridName = "grid";

        /// <summary>
        /// Fixture set up.
        /// </summary>
        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Ignition.Start(TestUtils.GetTestConfiguration());
            Ignition.Start(new IgniteConfiguration(TestUtils.GetTestConfiguration()) {GridName = SecondGridName });
        }

        /// <summary>
        /// Fixture tear down.
        /// </summary>
        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Ignition.StopAll(true);
        }

        /// <summary>
        /// Tests the local metrics.
        /// </summary>
        [Test]
        public void TestLocalMetrics()
        {
            var metrics = GetLocalRemoteMetrics("localMetrics", c => c.GetLocalMetrics());

            var localMetrics = metrics.Item1;
            var remoteMetrics = metrics.Item2;

            Assert.AreEqual(1, localMetrics.Size);
            Assert.AreEqual(1, localMetrics.CacheGets);
            Assert.AreEqual(1, localMetrics.CachePuts);

            Assert.AreEqual(0, remoteMetrics.Size);
            Assert.AreEqual(0, remoteMetrics.CacheGets);
            Assert.AreEqual(0, remoteMetrics.CachePuts);
        }

        /// <summary>
        /// Tests the global metrics.
        /// </summary>
        [Test]
        public void TestGlobalMetrics()
        {
            var metrics = GetLocalRemoteMetrics("globalMetrics", c => c.GetMetrics());

            var localMetrics = metrics.Item1;
            var remoteMetrics = metrics.Item2;

            Assert.AreEqual(1, localMetrics.Size);
            Assert.AreEqual(1, localMetrics.CacheGets);
            Assert.AreEqual(1, localMetrics.CachePuts);

            Assert.AreEqual(0, remoteMetrics.Size);
            Assert.AreEqual(1, remoteMetrics.CacheGets);
            Assert.AreEqual(1, remoteMetrics.CachePuts);
        }

        /// <summary>
        /// Tests the cluster group metrics.
        /// </summary>
        [Test]
        public void TestClusterGroupMetrics()
        {
            var cluster = Ignition.GetIgnite().GetCluster();

            // Get metrics in reverse way, so that first item is for second node and vice versa.
            var metrics = GetLocalRemoteMetrics("clusterMetrics", c => c.GetMetrics(cluster.ForRemotes()), 
                c => c.GetMetrics(cluster.ForLocal()));

            var localMetrics = metrics.Item2;
            var remoteMetrics = metrics.Item1;

            Assert.AreEqual(1, localMetrics.Size);
            Assert.AreEqual(1, localMetrics.CacheGets);
            Assert.AreEqual(1, localMetrics.CachePuts);

            Assert.AreEqual(1, remoteMetrics.Size);
            Assert.AreEqual(0, remoteMetrics.CacheGets);
            Assert.AreEqual(0, remoteMetrics.CachePuts);
        }

        /// <summary>
        /// Tests the metrics propagation.
        /// </summary>
        [Test]
        public void TestMetricsPropagation()
        {
            var ignite = Ignition.GetIgnite();

            using (var inStream = IgniteManager.Memory.Allocate().GetStream())
            {
                var result = ignite.GetCompute().ExecuteJavaTask<bool>(
                    "org.apache.ignite.platform.PlatformCacheWriteMetricsTask", inStream.MemoryPointer);

                Assert.IsTrue(result);

                inStream.SynchronizeInput();

                var reader = ((Ignite) ignite).Marshaller.StartUnmarshal(inStream);

                ICacheMetrics metrics = new CacheMetricsImpl(reader);

                Assert.AreEqual(1, metrics.CacheHits);
                Assert.AreEqual(2, metrics.CacheHitPercentage);
                Assert.AreEqual(3, metrics.CacheMisses);
                Assert.AreEqual(4, metrics.CacheMissPercentage);
                Assert.AreEqual(5, metrics.CacheGets);
                Assert.AreEqual(6, metrics.CachePuts);
                Assert.AreEqual(7, metrics.CacheRemovals);
                Assert.AreEqual(8, metrics.CacheEvictions);
                Assert.AreEqual(9, metrics.AverageGetTime);
                Assert.AreEqual(10, metrics.AveragePutTime);
                Assert.AreEqual(11, metrics.AverageRemoveTime);
                Assert.AreEqual(12, metrics.AverageTxCommitTime);
                Assert.AreEqual(13, metrics.AverageTxRollbackTime);
                Assert.AreEqual(14, metrics.CacheTxCommits);
                Assert.AreEqual(15, metrics.CacheTxRollbacks);
                Assert.AreEqual("myCache", metrics.CacheName);
                Assert.AreEqual(16, metrics.OverflowSize);
                Assert.AreEqual(17, metrics.OffHeapGets);
                Assert.AreEqual(18, metrics.OffHeapPuts);
                Assert.AreEqual(19, metrics.OffHeapRemovals);
                Assert.AreEqual(20, metrics.OffHeapEvictions);
                Assert.AreEqual(21, metrics.OffHeapHits);
                Assert.AreEqual(22, metrics.OffHeapHitPercentage);
                Assert.AreEqual(23, metrics.OffHeapMisses);
                Assert.AreEqual(24, metrics.OffHeapMissPercentage);
                Assert.AreEqual(25, metrics.OffHeapEntriesCount);
                Assert.AreEqual(26, metrics.OffHeapPrimaryEntriesCount);
                Assert.AreEqual(27, metrics.OffHeapBackupEntriesCount);
                Assert.AreEqual(28, metrics.OffHeapAllocatedSize);
                Assert.AreEqual(29, metrics.OffHeapMaxSize);
                Assert.AreEqual(30, metrics.SwapGets);
                Assert.AreEqual(31, metrics.SwapPuts);
                Assert.AreEqual(32, metrics.SwapRemovals);
                Assert.AreEqual(33, metrics.SwapHits);
                Assert.AreEqual(34, metrics.SwapMisses);
                Assert.AreEqual(35, metrics.SwapEntriesCount);
                Assert.AreEqual(36, metrics.SwapSize);
                Assert.AreEqual(37, metrics.SwapHitPercentage);
                Assert.AreEqual(38, metrics.SwapMissPercentage);
                Assert.AreEqual(39, metrics.Size);
                Assert.AreEqual(40, metrics.KeySize);
                Assert.AreEqual(true, metrics.IsEmpty);
                Assert.AreEqual(41, metrics.DhtEvictQueueCurrentSize);
                Assert.AreEqual(42, metrics.TxThreadMapSize);
                Assert.AreEqual(43, metrics.TxXidMapSize);
                Assert.AreEqual(44, metrics.TxCommitQueueSize);
                Assert.AreEqual(45, metrics.TxPrepareQueueSize);
                Assert.AreEqual(46, metrics.TxStartVersionCountsSize);
                Assert.AreEqual(47, metrics.TxCommittedVersionsSize);
                Assert.AreEqual(48, metrics.TxRolledbackVersionsSize);
                Assert.AreEqual(49, metrics.TxDhtThreadMapSize);
                Assert.AreEqual(50, metrics.TxDhtXidMapSize);
                Assert.AreEqual(51, metrics.TxDhtCommitQueueSize);
                Assert.AreEqual(52, metrics.TxDhtPrepareQueueSize);
                Assert.AreEqual(53, metrics.TxDhtStartVersionCountsSize);
                Assert.AreEqual(54, metrics.TxDhtCommittedVersionsSize);
                Assert.AreEqual(55, metrics.TxDhtRolledbackVersionsSize);
                Assert.AreEqual(true, metrics.IsWriteBehindEnabled);
                Assert.AreEqual(56, metrics.WriteBehindFlushSize);
                Assert.AreEqual(57, metrics.WriteBehindFlushThreadCount);
                Assert.AreEqual(58, metrics.WriteBehindFlushFrequency);
                Assert.AreEqual(59, metrics.WriteBehindStoreBatchSize);
                Assert.AreEqual(60, metrics.WriteBehindTotalCriticalOverflowCount);
                Assert.AreEqual(61, metrics.WriteBehindCriticalOverflowCount);
                Assert.AreEqual(62, metrics.WriteBehindErrorRetryCount);
                Assert.AreEqual(63, metrics.WriteBehindBufferSize);
                Assert.AreEqual("foo", metrics.KeyType);
                Assert.AreEqual("bar", metrics.ValueType);
                Assert.AreEqual(true, metrics.IsStoreByValue);
                Assert.AreEqual(true, metrics.IsStatisticsEnabled);
                Assert.AreEqual(true, metrics.IsManagementEnabled);
                Assert.AreEqual(true, metrics.IsReadThrough);
                Assert.AreEqual(true, metrics.IsWriteThrough);
            }
        }

        /// <summary>
        /// Creates a cache, performs put-get, returns metrics from both nodes.
        /// </summary>
        private static Tuple<ICacheMetrics, ICacheMetrics> GetLocalRemoteMetrics(string cacheName,
                    Func<ICache<int, int>, ICacheMetrics> func, Func<ICache<int, int>, ICacheMetrics> func2 = null)
        {
            func2 = func2 ?? func;

            var localCache = Ignition.GetIgnite().CreateCache<int, int>(new CacheConfiguration(cacheName)
            {
                EnableStatistics = true
            });

            var remoteCache = Ignition.GetIgnite(SecondGridName).GetCache<int, int>(cacheName);

            Assert.IsTrue(localCache.GetConfiguration().EnableStatistics);
            Assert.IsTrue(remoteCache.GetConfiguration().EnableStatistics);

            localCache.Put(1, 1);
            localCache.Get(1);
            
            // Wait for metrics to propagate.
            Thread.Sleep(TcpDiscoverySpi.DefaultHeartbeatFrequency);

            var localMetrics = func(localCache);
            Assert.IsTrue(localMetrics.IsStatisticsEnabled);
            Assert.AreEqual(cacheName, localMetrics.CacheName);

            var remoteMetrics = func2(remoteCache);
            Assert.IsTrue(remoteMetrics.IsStatisticsEnabled);
            Assert.AreEqual(cacheName, remoteMetrics.CacheName);

            return Tuple.Create(localMetrics, remoteMetrics);
        }
    }
}
