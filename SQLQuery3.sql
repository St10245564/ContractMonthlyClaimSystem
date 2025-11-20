-- Check if AuditLogs table exists and has correct structure
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AuditLogs')
BEGIN
    -- Check for NULL constraints and add defaults if needed
    IF COL_LENGTH('AuditLogs', 'OldValues') IS NOT NULL
    BEGIN
        -- Update existing NULL values to empty string
        UPDATE [dbo].[AuditLogs] SET [OldValues] = '' WHERE [OldValues] IS NULL;
    END
    
    IF COL_LENGTH('AuditLogs', 'NewValues') IS NOT NULL
    BEGIN
        -- Update existing NULL values to empty string
        UPDATE [dbo].[AuditLogs] SET [NewValues] = '' WHERE [NewValues] IS NULL;
    END
    
    IF COL_LENGTH('AuditLogs', 'IPAddress') IS NOT NULL
    BEGIN
        -- Update existing NULL values to empty string
        UPDATE [dbo].[AuditLogs] SET [IPAddress] = '' WHERE [IPAddress] IS NULL;
    END
END