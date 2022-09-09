alter table scriptycord.playlist_entries add audio_length bigint not null default 1;
alter table scriptycord.playlist_entries add source_identifier varchar(128) default '';