package com.smartsolar.microgrid.data.local

import android.provider.BaseColumns

object DatabaseContract {
    const val DATABASE_NAME = "smart_solar.db"
    const val DATABASE_VERSION = 1

    object AppMetadata : BaseColumns {
        const val TABLE_NAME = "app_metadata"
        const val COLUMN_KEY = "metadata_key"
        const val COLUMN_VALUE = "metadata_value"
        const val COLUMN_UPDATED_AT = "updated_at"

        const val CREATE_TABLE = """
            CREATE TABLE $TABLE_NAME (
                $COLUMN_KEY TEXT PRIMARY KEY NOT NULL,
                $COLUMN_VALUE TEXT NOT NULL,
                $COLUMN_UPDATED_AT INTEGER NOT NULL
            )
        """
    }
}

