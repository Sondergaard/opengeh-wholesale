# Read description in the 'views.dsl' file.

wholesaleDomain = group "Wholesale" {
    wholesaleDataLake = container "Data Lake (Wholesale)" {
        description "Stores batch results"
        technology "Azure Data Lake Gen 2"
        tags "Data Storage" "Microsoft Azure - Data Lake Store Gen1"
    }
    wholesaleCalculator = container "Calculation Engine" {
        description "Executes energy calculations"
        technology "Azure Databricks"
        tags "Microsoft Azure - Azure Databricks"

        # Domain relationships
        this -> wholesaleDataLake "read / write"
    }
    wholesaleDb = container "Wholesale Database" {
        description "Stores batches and operations data"
        technology "SQL Database Schema"
        tags "Data Storage" "Microsoft Azure - SQL Database"
    }
    wholesaleApi = container "Wholesale API" {
        description "Backend server providing external web API for wholesale operations"
        technology "Asp.Net Core Web API"
        tags "Microsoft Azure - App Services"

        # Base model relationships
        this -> dh3.sharedServiceBus "publishes events"

        # Domain relationships
        this -> wholesaleDb "uses" "EF Core"
        this -> wholesaleCalculator "sends requests to"
        this -> wholesaleDataLake "retrieves results from"

        # Domain-to-domain relationships
        this -> ediTimeSeriesListener "notify of <event>" "message/amqp" {
            tags "Simple View"
        }
    }
}

