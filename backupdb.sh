#!/bin/bash

# Configuration
export PATH="$PATH:/c/Program Files/MySQL/MySQL Server 9.2/bin" # change this to the path of your MySQL copy you use. It needs to have mysqldump in order to run this backup script.
DB_NAME="" # db name used
DB_USER="" # username, preferably an admin one that has full control
DB_PASSWORD="" # password for said account
BACKUP_DIR="/c/backups" # change to desired backup directory
mkdir -p $BACKUP_DIR # makes the dir if it doesn't exist
TIMESTAMP=$(date +"%F")
BACKUP_FILE="$BACKUP_DIR/${DB_NAME}_backup_$TIMESTAMP.sql"

# MySQL Dump Command
mysqldump -u "$DB_USER" -p"$DB_PASSWORD" "$DB_NAME" > "$BACKUP_FILE"

# Compression (optional)
gzip "$BACKUP_FILE"