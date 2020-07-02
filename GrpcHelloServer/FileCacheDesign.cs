using System.Collections.Generic;

class FileDataHandle
{
    public byte[] FileContent { get; }

    public bool IsCachedInMemory { get; }

    public string PathOnDisk { get; }
}

class FileCacheState
{
    string FileId;

    string CacheState; //?
}

class SiteCacheState
{
    public string SiteName { get; }

    public List<FileCacheState> FileCacheStates { get; }
}

class AgentStateSummary { }

interface ICacheService
{
    FileDataHandle GetFileForSite(string siteName, string fileId);
}

interface ICacheAgent
{
    void AssignSiteFile(string siteName, string fileId);

    void NotifySiteFileUpdated(string siteName, string fileId);

    List<SiteCacheState> GetAllCurrentCacheState();
}

interface ICacheAgentCoordinator
{
    void NotifyCacheAgentHeartbeat(string agentId, AgentStateSummary agentState);
}

class CacheNode : ICacheAgent, ICacheService
{
    void Initialize()
    {
        // Clear cache directory (?)

        // NotifyCacheAgentHeartbeat(...);
    }
}
