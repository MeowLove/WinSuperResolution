using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WinSuperResolution.Models
{
    [DataContract]
    public sealed class RegistryOperationJournal
    {
        public RegistryOperationJournal()
        {
            Entries = new List<RegistryJournalEntry>();
        }

        [DataMember] public string JournalId { get; set; }
        [DataMember] public string OperationType { get; set; }
        [DataMember] public string Status { get; set; }
        [DataMember] public DateTime CreatedUtc { get; set; }
        [DataMember] public string BackupPath { get; set; }
        [DataMember] public string ConfigurationKey { get; set; }
        [DataMember] public IList<RegistryJournalEntry> Entries { get; private set; }
    }

    [DataContract]
    public sealed class RegistryJournalEntry
    {
        [DataMember] public string RegistryPath { get; set; }
        [DataMember] public string WidthValueName { get; set; }
        [DataMember] public string HeightValueName { get; set; }
        [DataMember] public int OriginalWidth { get; set; }
        [DataMember] public int OriginalHeight { get; set; }
        [DataMember] public int TargetWidth { get; set; }
        [DataMember] public int TargetHeight { get; set; }
        [DataMember] public string ValueKind { get; set; }
        [DataMember] public string OriginalWidthBytes { get; set; }
        [DataMember] public string OriginalHeightBytes { get; set; }
        [DataMember] public string TargetWidthBytes { get; set; }
        [DataMember] public string TargetHeightBytes { get; set; }
        [DataMember] public bool Modified { get; set; }
        [DataMember] public string RecoveryStatus { get; set; }
    }

    public sealed class OperationResult
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public string JournalPath { get; set; }
        public string BackupPath { get; set; }
    }

    [DataContract]
    public sealed class DisplayModeSnapshot
    {
        [DataMember] public string DeviceName { get; set; }
        [DataMember] public int Width { get; set; }
        [DataMember] public int Height { get; set; }
        [DataMember] public int Frequency { get; set; }
        [DataMember] public int BitsPerPixel { get; set; }
        [DataMember] public DateTime CreatedUtc { get; set; }
        [DataMember] public bool Confirmed { get; set; }
    }
}
