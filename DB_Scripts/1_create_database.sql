CREATE DATABASE [SE_ActivityCollector];

Use [SE_ActivityCollector];

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

CREATE TABLE [dbo].[users] (
	[steam_id] [char](17) NOT NULL,
	CONSTRAINT PK_users PRIMARY KEY ([steam_id])
);

CREATE TABLE [dbo].[user_names](
	[steam_id] [char](17) NOT NULL,
	[username] [varchar](35) NOT NULL,
	[timestamp] [datetime] NOT NULL,
	CONSTRAINT FK_usernames_users FOREIGN KEY([steam_id])
	REFERENCES [dbo].[users] ([steam_id])
);

CREATE TABLE [dbo].[user_spawns]
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

CREATE TABLE [dbo].[user_piloting]
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

CREATE TABLE [dbo].[user_activity](
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

CREATE TABLE [dbo].[entities](
	[id] [char](20) NOT NULL,
	[session_id] [int] NOT NULL,
	[name] [varchar](128) NOT NULL,
	[object_type] [varchar](50) NOT NULL,
	[type_id] [varchar](50) NOT NULL,
	[subtype_id] [varchar](50) NOT NULL,
	[created] [datetime] NOT NULL,
	[removed] [datetime] NULL,
	CONSTRAINT [FK_entities_sessions] FOREIGN KEY([session_id])
	REFERENCES [dbo].[sessions] ([id])
);

CREATE TABLE [dbo].[entity_inventories](
	[entity_id] [char](20) NOT NULL,
	[block_id] [char](20) NULL,
	[item_id] [int] NOT NULL,
	[amount] [decimal](20, 3) NOT NULL,
	[durability] [decimal](20, 3) NULL,
	[type_id] [varchar](128) NULL,
	[subtype_id] [varchar](128) NULL,
	[session_id] [int] NOT NULL,
	[timestamp] [datetime] NOT NULL,
	CONSTRAINT [FK_entity_inventories_sessions] FOREIGN KEY([session_id])
	REFERENCES [dbo].[sessions] ([id])
);

CREATE TABLE grids
(
	[id] [char](20) NOT NULL,
	[parent_id] [char](20) NULL,
	[session_id] [int] NOT NULL,
	[type] [char](5) NOT NULL,
	[created] [datetime] NOT NULL,
	[removed] [datetime] NULL,
	[split_with_parent] [datetime] NULL,
	CONSTRAINT PK_grids PRIMARY KEY ([id], [session_id]),
	CONSTRAINT [FK_grids_sessions] FOREIGN KEY([session_id])
	REFERENCES [dbo].[sessions] ([id])
);

CREATE TABLE grid_names
(
	[grid_id] [char](20) NOT NULL,
	[session_id] [int] NOT NULL,
	[name] [nvarchar](128) NOT NULL,
	[timestamp] [datetime] NOT NULL
	CONSTRAINT [FK_grid_names_sessions] FOREIGN KEY([session_id])
	REFERENCES [dbo].[sessions] ([id])
);

CREATE TABLE [dbo].[grid_blocks](
	[id] [varchar](30) NOT NULL,
	[entity_id] [char](20) NULL,
	[grid_id] [char](20) NOT NULL,
	[session_id] [int] NOT NULL,
	[built_by] [char](20) NOT NULL,
	[type_id] [varchar](50) NOT NULL,
	[subtype_id] [varchar](50) NOT NULL,
	[x] [int] NOT NULL,
	[y] [int] NOT NULL,
	[z] [int] NOT NULL,
	[created] [datetime] NOT NULL,
	[removed] [datetime] NULL,
	CONSTRAINT [FK_grid_blocks_sessions] FOREIGN KEY([session_id])
	REFERENCES [dbo].[sessions] ([id])
);

CREATE TABLE [dbo].[grid_block_ownership](
	[block_id] [varchar](30) NOT NULL,
	[grid_id] [char](20) NOT NULL,
	[session_id] [int] NOT NULL,
	[owner] [char](20) NOT NULL,
	[timestamp] [datetime] NOT NULL,
	CONSTRAINT [FK_grid_block_ownership_sessions] FOREIGN KEY([session_id])
	REFERENCES [dbo].[sessions] ([id])
);

CREATE TABLE [dbo].[definition_blocks](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[session_id] [int] NOT NULL,
	[type_id] [varchar](128) NOT NULL,
	[subtype_id] [varchar](128) NOT NULL,
	[name] [varchar](128) NOT NULL,
	[cube_size] [char](5) NOT NULL,
	[max_integrity] [decimal](22, 7) NOT NULL,
	[critical_integrity_ratio] [decimal](22, 7) NOT NULL,
	[general_damage_multiplier] [decimal](22, 7) NOT NULL,
	[disassemble_ratio] [decimal](22, 7) NOT NULL,
	[deformation_ratio] [decimal](22, 7) NOT NULL,
	[mass] [decimal](22, 7) NOT NULL,
	[is_air_tight] [bit] NOT NULL,
	[size_x] [decimal](22, 7) NOT NULL,
	[size_y] [decimal](22, 7) NOT NULL,
	[size_z] [decimal](22, 7) NOT NULL,
	[model_offset_x] [decimal](22, 7) NOT NULL,
	[model_offset_y] [decimal](22, 7) NOT NULL,
	[model_offset_z] [decimal](22, 7) NOT NULL,
	[description] [text] NULL,
	[timestamp] [datetime] NOT NULL,
	CONSTRAINT [PK_block_definitions] PRIMARY KEY ([id]),
	CONSTRAINT [FK_definition_blocks_sessions] FOREIGN KEY([session_id])
	REFERENCES [dbo].[sessions] ([id])
);

CREATE TABLE [dbo].[definition_components](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[session_id] [int] NOT NULL,
	[type_id] [varchar](128) NOT NULL,
	[subtype_id] [varchar](128) NOT NULL,
	[name] [varchar](128) NOT NULL,
	[mass] [decimal](22, 7) NOT NULL,
	[volume] [decimal](22, 7) NOT NULL,
	[max_integrity] [decimal](22, 7) NOT NULL,
	[max_stack_amount] [decimal](22, 7) NOT NULL,
	[health] [decimal](22, 7) NOT NULL,
	[description] [text] NULL,
	[timestamp] [datetime] NOT NULL,
	CONSTRAINT [PK_definition_component] PRIMARY KEY ([id]),
	CONSTRAINT [FK_definition_components_sessions] FOREIGN KEY([session_id])
	REFERENCES [dbo].[sessions] ([id])
);

CREATE TABLE [dbo].[definition_block_components](
	[session_id] [int] NOT NULL,
	[block_id] [int] NOT NULL,
	[component_id] [int] NOT NULL,
	[index] [int] NOT NULL,
	[count] [int] NOT NULL,
	[timestamp] [datetime] NOT NULL,
	CONSTRAINT [FK_definition_block_components_definition_blocks] FOREIGN KEY([block_id])
	REFERENCES [dbo].[definition_blocks] ([id]),
	CONSTRAINT [FK_definition_block_components_definition_components] FOREIGN KEY([component_id])
	REFERENCES [dbo].[definition_components] ([id]),
	CONSTRAINT [FK_definition_block_components_sessions] FOREIGN KEY([session_id])
	REFERENCES [dbo].[sessions] ([id])
);

CREATE TABLE [dbo].[combatlog](
	[session_id] [int] NOT NULL,
	[attacker_grid_id] [char](20) NULL,
	[attacker_grid_block_id] [varchar](30) NULL,
	[attacker_entity_id] [char](20) NULL,
	[victim_grid_id] [char](20) NULL,
	[victim_grid_block_id] [varchar](30) NULL,
	[victim_entity_id] [char](20) NULL,
	[type] [varchar](50) NOT NULL,
	[damage] [decimal](20, 3) NOT NULL,
	[integrity] [decimal](20, 3) NOT NULL,
	[timestamp] [datetime] NOT NULL,
	CONSTRAINT [FK_combatlog_sessions] FOREIGN KEY([session_id])
	REFERENCES [dbo].[sessions] ([id])
);

CREATE TABLE [dbo].[factions](
	[faction_id] [char](20) NOT NULL,
	[iteration_id] [int] NOT NULL,
	[tag] [nchar](5) NOT NULL,
	[name] [nvarchar](128) NOT NULL,
	[description] [text] NULL,
	[creation_date] [datetime] NOT NULL,
	[termination_date] [datetime] NULL,
	CONSTRAINT [FK_factions_iterations] FOREIGN KEY([iteration_id])
	REFERENCES [dbo].[iterations] ([id])
);

CREATE TABLE [dbo].[faction_activity](
	[faction_activity_id] [int] IDENTITY(1,1) NOT NULL,
	[action] [varchar](30) NOT NULL,
	[session_id] [int] NOT NULL,
	[from_faction] [char](20) NOT NULL,
	[to_faction] [char](20) NOT NULL,
	[player_id] [char](20) NOT NULL,
	[sender_id] [char](20) NOT NULL,
	[timestamp] [datetime] NOT NULL,
	CONSTRAINT [FK_faction_activity_sessions] FOREIGN KEY([session_id])
	REFERENCES [dbo].[sessions] ([id])
);

INSERT INTO [dbo].[iterations]  ([id], [name], [type], [startdate])
	VALUES ('0', 'Default', 'None', GETDATE());

INSERT INTO [dbo].[users] ([steam_id])
	VALUES ('0'), ('1'), ('00000000000000000')

INSERT INTO [dbo].[user_names] ([steam_id], [username], [timestamp])
	VALUES ('0', 'NPC', GETDATE()),
		('1', 'Environment', GETDATE()),
		('00000000000000000', 'Server', GETDATE())