﻿[<AutoOpen>]
module Donald.Transaction

open System.Data
open System.Data.Common

type IDbTransaction with
    member internal this.NewCommand(commandType : CommandType, sql : string) =
        let cmd = this.Connection.NewCommand(commandType, sql)
        cmd.Transaction <- this        
        cmd

    member internal this.NewDbCommand(commandType : CommandType, sql : string) =
        this.NewCommand(commandType, sql) :?> DbCommand

    member this.TryRollback() =
        try        
            if not(isNull this) 
               && not(isNull this.Connection) then this.Rollback()
        with ex  -> 
            raise (CouldNotRollbackTransactionError ex) 

    member this.TryCommit() =
        try
            if not(isNull this) 
               && not(isNull this.Connection) then this.Commit() 
        with ex  -> 
            /// Is supposed to throw System.InvalidOperationException
            /// when commmited or rolled back already, but most
            /// implementations do not. So in all cases try rolling back
            this.TryRollback()
            raise (CouldNotCommitTransactionError ex)             