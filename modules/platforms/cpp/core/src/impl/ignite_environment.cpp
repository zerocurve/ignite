/*
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

#include "ignite/impl/interop/interop_external_memory.h"
#include "ignite/impl/binary/binary_reader_impl.h"
#include "ignite/impl/ignite_environment.h"
#include "ignite/binary/binary.h"
#include "ignite/impl/binary/binary_type_updater_impl.h"

using namespace ignite::common::concurrent;
using namespace ignite::jni::java;
using namespace ignite::impl::interop;
using namespace ignite::impl::binary;
using namespace ignite::binary;

namespace ignite 
{
    namespace impl 
    {
        /**
         * OnStart callback.
         *
         * @param target Target environment.
         * @param proc Processor instance.
         * @param memPtr Memory pointer.
         */
        void IGNITE_CALL OnStart(void* target, void* proc, long long memPtr)
        {
            SharedPointer<IgniteEnvironment>* ptr = static_cast<SharedPointer<IgniteEnvironment>*>(target);

            ptr->Get()->OnStartCallback(memPtr, reinterpret_cast<jobject>(proc));
        }

        /**
         * OnStop callback.
         *
         * @param target Target environment.
         */
        void IGNITE_CALL OnStop(void* target)
        {
            SharedPointer<IgniteEnvironment>* ptr = static_cast<SharedPointer<IgniteEnvironment>*>(target);

            delete ptr;
        }

        /**
         * Memory reallocate callback.
         *
         * @param target Target environment.
         * @param memPtr Memory pointer.
         * @param cap Required capasity.
         */
        void IGNITE_CALL MemoryReallocate(void* target, long long memPtr, int cap)
        {
            SharedPointer<IgniteEnvironment>* env = static_cast<SharedPointer<IgniteEnvironment>*>(target);

            SharedPointer<InteropMemory> mem = env->Get()->GetMemory(memPtr);

            mem.Get()->Reallocate(cap);
        }

        IgniteEnvironment::IgniteEnvironment() : ctx(SharedPointer<JniContext>()), latch(new SingleLatch), name(0),
            proc(), metaMgr(new BinaryTypeManager()), metaUpdater(0)
        {
            // No-op.
        }

        IgniteEnvironment::~IgniteEnvironment()
        {
            delete latch;
            delete name;
            delete metaMgr;
            delete metaUpdater;
        }

        JniHandlers IgniteEnvironment::GetJniHandlers(SharedPointer<IgniteEnvironment>* target)
        {
            JniHandlers hnds = JniHandlers();

            hnds.target = target;

            hnds.onStart = OnStart;
            hnds.onStop = OnStop;

            hnds.memRealloc = MemoryReallocate;

            hnds.error = 0;

            return hnds;
        }

        void IgniteEnvironment::SetContext(SharedPointer<JniContext> ctx)
        {
            this->ctx = ctx;
        }

        void IgniteEnvironment::Initialize()
        {
            latch->CountDown();

            jobject binaryProc = Context()->ProcessorBinaryProcessor(proc.Get());

            metaUpdater = new BinaryTypeUpdaterImpl(*this, binaryProc);
        }

        const char* IgniteEnvironment::InstanceName() const
        {
            return name;
        }

        JniContext* IgniteEnvironment::Context()
        {
            return ctx.Get();
        }

        SharedPointer<InteropMemory> IgniteEnvironment::AllocateMemory()
        {
            SharedPointer<InteropMemory> ptr(new InteropUnpooledMemory(DEFAULT_ALLOCATION_SIZE));

            return ptr;
        }

        SharedPointer<InteropMemory> IgniteEnvironment::AllocateMemory(int32_t cap)
        {
            SharedPointer<InteropMemory> ptr(new InteropUnpooledMemory(cap));

            return ptr;
        }

        SharedPointer<InteropMemory> IgniteEnvironment::GetMemory(int64_t memPtr)
        {
            int8_t* memPtr0 = reinterpret_cast<int8_t*>(memPtr);

            int32_t flags = InteropMemory::Flags(memPtr0);

            if (InteropMemory::IsExternal(flags))
            {
                SharedPointer<InteropMemory> ptr(new InteropExternalMemory(memPtr0));

                return ptr;
            }
            else
            {
                SharedPointer<InteropMemory> ptr(new InteropUnpooledMemory(memPtr0));

                return ptr;
            }
        }

        BinaryTypeManager* IgniteEnvironment::GetTypeManager()
        {
            return metaMgr;
        }

        BinaryTypeUpdater* IgniteEnvironment::GetTypeUpdater()
        {
            return metaUpdater;
        }

        void IgniteEnvironment::ProcessorReleaseStart()
        {
            if (proc.Get())
                ctx.Get()->ProcessorReleaseStart(proc.Get());
        }

        void IgniteEnvironment::OnStartCallback(long long memPtr, jobject proc)
        {
            this->proc = jni::JavaGlobalRef(*ctx.Get(), proc);

            InteropExternalMemory mem(reinterpret_cast<int8_t*>(memPtr));
            InteropInputStream stream(&mem);

            BinaryReaderImpl reader(&stream);

            int32_t nameLen = reader.ReadString(0, 0);

            if (nameLen >= 0)
            {
                name = new char[nameLen + 1];
                reader.ReadString(name, nameLen + 1);
            }
            else
                name = 0;
        }
    }
}





