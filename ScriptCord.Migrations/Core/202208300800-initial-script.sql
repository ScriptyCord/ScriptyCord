create schema scriptcord;

create extension "uuid-ossp";

SET TIME ZONE 'UTC';

-- Music Bot
drop table if exists scriptcord.playlist_entries;
drop table if exists scriptcord.playlists;

create table scriptcord.playlists (
	id serial primary key not null,
	guild_id bigint not null,
	"name" varchar(80) not null,
	is_default boolean not null default false,
	admin_only boolean not null default false
);

create table scriptcord.playlist_entries (
	id uuid primary key default uuid_generate_v4(),
	playlist_id integer not null,
	title varchar(150) not null,
	source varchar(30) not null,
	
	constraint fk_playlist foreign key(playlist_id) references scriptcord.playlists(id)
);