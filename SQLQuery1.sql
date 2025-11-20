CREATE TABLE [dbo].[AuditLogs] (
    [AuditLogId]  INT             IDENTITY (1, 1) NOT NULL,
    [Action]      NVARCHAR (50)   NOT NULL,
    [TableName]   NVARCHAR (50)   NOT NULL,
    [RecordId]    INT             NOT NULL,
    [OldValues]   NVARCHAR (MAX)  NULL,
    [NewValues]   NVARCHAR (MAX)  NULL,
    [ChangedBy]   INT             NOT NULL,
    [ChangedDate] DATETIME2 (7)   DEFAULT (getdate()) NOT NULL,
    [IPAddress]   NVARCHAR (45)   NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY CLUSTERED ([AuditLogId] ASC),
    CONSTRAINT [FK_AuditLogs_Users_ChangedBy] FOREIGN KEY ([ChangedBy]) REFERENCES [dbo].[Users] ([UserId])
);


GO
CREATE NONCLUSTERED INDEX [IX_AuditLogs_ChangedBy]
    ON [dbo].[AuditLogs]([ChangedBy] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_AuditLogs_ChangedDate]
    ON [dbo].[AuditLogs]([ChangedDate] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_AuditLogs_TableName_RecordId]
    ON [dbo].[AuditLogs]([TableName] ASC, [RecordId] ASC);