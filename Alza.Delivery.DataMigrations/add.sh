#!/bin/bash

if [ -z "$1" ]; then
    echo "Usage: ./add.sh [MigrationName]"
    exit 1
fi

dotnet ef migrations add "$1" --context DeliveryDbContext -o Migrations
