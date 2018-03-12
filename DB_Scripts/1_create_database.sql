CREATE DATABASE [SE_ActivityCollector];

Use [SE_ActivityCollector];

CREATE TABLE [dbo].[users] (
	[steam_id] [char](17) NOT NULL,
	CONSTRAINT PK_users PRIMARY KEY ([steam_id])
);

CREATE TABLE [dbo].[usernames](
	[steam_id] [char](17) NOT NULL,
	[username] [varchar](35) NOT NULL,
	[timestamp] [datetime] NOT NULL,
	CONSTRAINT FK_usernames_users FOREIGN KEY([steam_id])
	REFERENCES [dbo].[users] ([steam_id])
);

CREATE TABLE [dbo].[iterations](
	[id] int NOT NULL,
	[name] [nvarchar](50) NULL,
	[type] [varchar](25) NULL,
	[startdate] [datetime] NOT NULL,
	[description] [text] NULL,
	CONSTRAINT PK_iterations PRIMARY KEY ([id])
);

CREATE TABLE [dbo].[sessions](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[iteration_id] [int] NULL,
	[status] [varchar](8) NOT NULL,
	[timestamp] [datetime] NOT NULL,
	CONSTRAINT PK_sessions PRIMARY KEY ([id]),
	CONSTRAINT FK_iteration_sessions FOREIGN KEY([iteration_id]) 
	REFERENCES [dbo].[iterations] ([id])
);

--CREATE TABLE [dbo].[kills](
--	[id] [int] IDENTITY(1,1) NOT NULL,
--	[session_id] [int] NOT NULL,
--	[killer_id] [char](17) NULL,
--	[weapon] [nvarchar](128) NULL,
--	[killer_was_piloting] [bit] NULL,
--	[killer_grid_id] [char] (20) NULL,
--	[killer_grid_name] [nvarchar](128) NULL,
--	[victim_id] [char](17) NULL,
--	[victim_was_piloting] [bit] NULL,
--	[victim_grid_id] [char] (20) NULL,
--	[victim_grid_name] [nvarchar](128) NULL,
--	[timestamp] [datetime] NULL,
--	CONSTRAINT FK_kills_sessions FOREIGN KEY([session_id]) 
--	REFERENCES [dbo].[sessions] ([id])
--);

CREATE TABLE [dbo].[chatlog](
	[steam_id] [char](17) NOT NULL,
	[session_id] [int] NOT NULL,
	[message] [text] NOT NULL,
	[timestamp] [datetime] NOT NULL,
	CONSTRAINT FK_chatlog_sessions FOREIGN KEY([session_id]) 
	REFERENCES [dbo].[sessions] ([id]),
	CONSTRAINT FK_chatlog_users FOREIGN KEY([steam_id]) 
	REFERENCES [dbo].[users] ([steam_id])
);

CREATE TABLE [dbo].[activity](
	[steam_id] [char](17) NOT NULL,
	[player_id] [char] (20) NOT NULL,
	[session_id] [int] NOT NULL,
	[connected] [datetime] NULL,
	[disconnected] [datetime] NULL,
	[state] [varchar](20) NOT NULL,
	[blocked_id] [char](17) NULL,
	CONSTRAINT FK_activity_sessions FOREIGN KEY([session_id]) 
	REFERENCES [dbo].[sessions] ([id]),
	CONSTRAINT FK_activity_users FOREIGN KEY([steam_id]) 
	REFERENCES [dbo].[users] ([steam_id])
);

CREATE TABLE grids
(
	[id] [char] (20) NOT NULL,
	[parent_id] [char] (20) NULL,
	[iteration_id] [int] NOT NULL,
	[type] [char] (5) NOT NULL,
	[created] [datetime] NOT NULL,
	[removed] [datetime] NULL,
	[split_with_parent] [datetime] NULL,
	CONSTRAINT PK_grids PRIMARY KEY ([id], [iteration_id]),
	CONSTRAINT FK_grids_iterations FOREIGN KEY ([iteration_id])
	REFERENCES [dbo].[iterations] (id) 
);

CREATE TABLE grid_names
(
	[grid_id] [varchar] (20) NOT NULL,
	[iteration_id] [int] NOT NULL,
	[name] [nvarchar](128) NOT NULL,
	[timestamp] [datetime] NOT NULL,
	CONSTRAINT FK_grid_names_grids FOREIGN KEY ([grid_id], [iteration_id])
	REFERENCES [dbo].grids ([id], [iteration_id])
);

CREATE TABLE blocks 
(
	[id] [char](20) NOT NULL,
	[grid_id] [char](20) NOT NULL,
	[iteration_id] [int] NOT NULL,
	[built_by] [char] (20) NOT NULL,
	[name] [varchar] (128) NOT NULL,
	[type] [varchar] (4) NOT NULL,
	[max_integrity] [decimal] NOT NULL,
	[x] [int] NOT NULL,
	[y] [int] NOT NULL,
	[z] [int] NOT NULL,
	CONSTRAINT PK_blocks PRIMARY KEY ([id], [grid_id], [iteration_id]),
	CONSTRAINT FK_blocks_grids FOREIGN KEY ([grid_id], [iteration_id])
	REFERENCES [dbo].[grids] ([id], [iteration_id])
);

CREATE TABLE spawns
(
	[steam_id] [char] (17) NOT NULL,
	[player_id] [char] (20) NOT NULL,
	[character_id] [char] (20) NOT NULL,
	[session_id] [int] NOT NULL,
	[start_time] [datetime] NULL,
	[end_time] [datetime] NULL,
	CONSTRAINT FK_players_sessions FOREIGN KEY([session_id]) 
	REFERENCES [dbo].[sessions] ([id]),
	CONSTRAINT FK_players_users FOREIGN KEY([steam_id]) 
	REFERENCES [dbo].[users] ([steam_id])
);

CREATE TABLE piloting
(
	[grid_id] [char] (20) NOT NULL,
	[player_id] [char] (20) NOT NULL,
	[session_id] [int] NOT NULL,
	[start_time] [datetime] NOT NULL,
	[end_time] [datetime] NULL,
	[is_piloting] [bit] NOT NULL,
	CONSTRAINT FK_piloting_sessions FOREIGN KEY([session_id]) 
	REFERENCES [dbo].[sessions] ([id]),
);

CREATE TABLE [dbo].[combatlog] 
(
	[session_id] [int] NOT NULL,
	[attacker_grid_id] [char] (20) NULL,
	[attacker_grid_name] [nvarchar] (128) NULL,
	[attacker_grid_owner] [char] (20) NULL,
	[attacker_entity_id] [char] (20) NULL,
	[attacker_entity_type] [varchar] (50) NULL,
	[attacker_entity_subtype] [varchar] (50) NULL,
	[attacker_entity_name] [nvarchar] (128) NULL,
	[attacker_entity_object_type] [varchar] (50) NULL,
	[attacker_entity_owner] [char] (20) NULL,
	[victim_grid_id] [char] (20) NULL,
	[victim_grid_name] [nvarchar] (128) NULL,
	[victim_grid_owner] [char] (20) NULL,
	[victim_entity_id] [char] (20) NULL,
	[victim_entity_type] [varchar] (50) NULL,
	[victim_entity_subtype] [varchar] (50) NULL, 
	[victim_entity_object_type] [varchar] (50) NULL,
	[victim_entity_name] [nvarchar] (128) NULL,
	[victim_entity_owner] [char] (20) NULL,
	[damage_type] [varchar] (50) NOT NULL,
	[damage] [decimal] NOT NULL,
	[target_entity_functional] [bit] NOT NULL,
	[target_entity_destroyed] [bit] NOT NULL,
	[timestamp] [datetime] NOT NULL,
	CONSTRAINT FK_combat_sessions FOREIGN KEY([session_id]) 
	REFERENCES [dbo].[sessions] ([id])
);

CREATE TABLE [dbo].[factions]
(
	[faction_id] [char] (20) NOT NULL,
	[iteration_id] [int] NOT NULL,
	[tag] [nchar] (5) NOT NULL,
	[name] [nvarchar] (128) NOT NULL,
	[description] [text] NULL,
	[creation_date] [datetime] NOT NULL,
	[termination_date] [datetime] NULL
);

CREATE TABLE [dbo].[faction_activity] 
(
	[faction_activity_id] [int] IDENTITY(1,1) NOT NULL,
	[action] [varchar] (30) NOT NULL,
	[session_id] [int] NOT NULL,
	[from_faction] [char] (20) NOT NULL,
	[to_faction] [char] (20) NOT NULL,
	[player_id] [char] (20) NOT NULL,
	[sender_id] [char] (20) NOT NULL,
	[timestamp] [datetime] NOT NULL
);

INSERT INTO [dbo].[iterations]  ([id], [name], [type], [startdate])
	VALUES ('0', 'Default', 'None', GETDATE());

INSERT INTO [dbo].[users] ([steam_id])
	VALUES ('0'), ('1'), ('00000000000000000')

INSERT INTO [dbo].[usernames] ([steam_id], [username], [timestamp])
	VALUES ('0', 'NPC', GETDATE()),
		('1', 'Environment', GETDATE()),
		('00000000000000000', 'Server', GETDATE())