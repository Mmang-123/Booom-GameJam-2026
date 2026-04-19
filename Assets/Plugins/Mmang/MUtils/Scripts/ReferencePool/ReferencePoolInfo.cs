using System;

namespace Mmang.Util
{
    public struct ReferencePoolInfo
    {
        private readonly Type m_Type;

        private readonly int m_UnusedReferenceCount;

        private readonly int m_UsingReferenceCount;

        private readonly int m_AcquireReferenceCount;

        private readonly int m_ReleaseReferenceCount;

        private readonly int m_AddReferenceCount;

        private readonly int m_RemoveReferenceCount;

        //
        // 摘要:
        //     获取引用池类型。
        public Type Type => m_Type;

        //
        // 摘要:
        //     获取未使用引用数量。
        public int UnusedReferenceCount => m_UnusedReferenceCount;

        //
        // 摘要:
        //     获取正在使用引用数量。
        public int UsingReferenceCount => m_UsingReferenceCount;

        //
        // 摘要:
        //     获取获取引用数量。
        public int AcquireReferenceCount => m_AcquireReferenceCount;

        //
        // 摘要:
        //     获取归还引用数量。
        public int ReleaseReferenceCount => m_ReleaseReferenceCount;

        //
        // 摘要:
        //     获取增加引用数量。
        public int AddReferenceCount => m_AddReferenceCount;

        //
        // 摘要:
        //     获取移除引用数量。
        public int RemoveReferenceCount => m_RemoveReferenceCount;

        //
        // 摘要:
        //     初始化引用池信息的新实例。
        //
        // 参数:
        //   type:
        //     引用池类型。
        //
        //   unusedReferenceCount:
        //     未使用引用数量。
        //
        //   usingReferenceCount:
        //     正在使用引用数量。
        //
        //   acquireReferenceCount:
        //     获取引用数量。
        //
        //   releaseReferenceCount:
        //     归还引用数量。
        //
        //   addReferenceCount:
        //     增加引用数量。
        //
        //   removeReferenceCount:
        //     移除引用数量。
        public ReferencePoolInfo(Type type, int unusedReferenceCount, int usingReferenceCount, int acquireReferenceCount, int releaseReferenceCount, int addReferenceCount, int removeReferenceCount)
        {
            m_Type = type;
            m_UnusedReferenceCount = unusedReferenceCount;
            m_UsingReferenceCount = usingReferenceCount;
            m_AcquireReferenceCount = acquireReferenceCount;
            m_ReleaseReferenceCount = releaseReferenceCount;
            m_AddReferenceCount = addReferenceCount;
            m_RemoveReferenceCount = removeReferenceCount;
        }
    }
}