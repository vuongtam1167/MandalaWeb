create database Mandala

CREATE TABLE [User]
(
    [ID] BIGINT IDENTITY(1,1) NOT NULL,
    [UserName] NVARCHAR(50),
    [Password] NVARCHAR(32),
    [Name] NVARCHAR(50),
    [Email] NVARCHAR(50),
	[Avatar] NVARCHAR(MAX),
    [CreatedDate] DATETIME,
    [Status] BIT DEFAULT 1,
    CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED ([ID])
);

CREATE TABLE [Mandala]
(
    [ID] BIGINT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(50),
	[Class] INT NULL,
    [CreatedDate] DATETIME,
	[CreatedUserID] BIGINT,
	[ModifiedDate] DATETIME,
	[ModifiedUserID] BIGINT,
    [Status] BIT DEFAULT 1,
    CONSTRAINT [PK_Mandala] PRIMARY KEY CLUSTERED ([ID])
);

CREATE TABLE [Mandala_Target]
(
    [MandalaLv] BIGINT,
	[MandalaID] BIGINT,
    [Target] NVARCHAR(250),
    CONSTRAINT [PK_Mandala_Target] PRIMARY KEY ([MandalaLv],[MandalaID])
);

CREATE TABLE [Mandala_Detail]
(
	[ID] BIGINT IDENTITY(1,1) NOT NULL,
    [MandalaLv] BIGINT,
	[MandalaID] BIGINT,
	[Deadline] DATETIME,
	[Status] BIT DEFAULT 1,
	[Action] NVARCHAR(250),
	[Result] NVARCHAR(250),
	[Person] NVARCHAR(50),
    CONSTRAINT [PK_Mandala_Detail] PRIMARY KEY CLUSTERED ([ID])
);

CREATE TABLE [MandalaShare]
(
    [MandalaID] BIGINT NOT NULL,
    [SharedUserID] BIGINT NOT NULL,
    [Permission] NVARCHAR(50) DEFAULT 'read',
    [SharedDate] DATETIME DEFAULT GETDATE(),
    [SharedBy] BIGINT,
    CONSTRAINT [PK_MandalaShare] PRIMARY KEY CLUSTERED ([MandalaID], [SharedUserID])
);

ALTER TABLE [Mandala]
ADD CONSTRAINT [FK_Mandala_CreatedUserID]
FOREIGN KEY ([CreatedUserID])
REFERENCES [User] ([ID]);

ALTER TABLE [Mandala]
ADD CONSTRAINT [FK_Mandala_ModifiedUserID]
FOREIGN KEY ([ModifiedUserID])
REFERENCES [User] ([ID]);

ALTER TABLE [Mandala_Target]
ADD CONSTRAINT [FK_MandalaTarget_MandalaID]
FOREIGN KEY ([MandalaID])
REFERENCES [Mandala] ([ID]);

ALTER TABLE [Mandala_Detail]
ADD CONSTRAINT [FK_MandalaDetail_MandalaID_MandalaLv]
FOREIGN KEY ([MandalaLv],[MandalaID])
REFERENCES [Mandala_Target] ([MandalaLv],[MandalaID]);

ALTER TABLE [MandalaShare]
ADD CONSTRAINT [FK_MandalaShare_MandalaID]
FOREIGN KEY ([MandalaID])
REFERENCES [Mandala] ([ID]);

ALTER TABLE [MandalaShare]
ADD CONSTRAINT [FK_MandalaShare_User]
FOREIGN KEY ([SharedUserID])
REFERENCES [User] ([ID]);