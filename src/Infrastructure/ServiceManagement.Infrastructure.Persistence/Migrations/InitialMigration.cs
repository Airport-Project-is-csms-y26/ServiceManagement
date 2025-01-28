using FluentMigrator;

namespace ServiceManagement.Infrastructure.Persistence.Migrations;

[Migration(1, "Initial")]
public class InitialMigration : Migration
{
    public override void Up()
    {
        string sql = """
                     create type task_state as enum ('planned', 'running', 'completed', 'cancelled');
                     create type task_type as enum ('technical_inspection', 'refueling', 'cleaning', 'repair', 'loading');

                     CREATE TABLE Tasks (
                          task_id BIGINT primary key generated always as identity,
                          task_flight_id BIGINT NOT NULL,
                          task_plane_number BIGINT not null,
                          task_type task_type NOT NULL,
                          task_status task_state NOT NULL,
                          task_executor VARCHAR(255) NOT NULL,
                          task_start_time timestamp with time zone NOT NULL
                      );
                     """;
        Execute.Sql(sql);
    }

    public override void Down()
    {
        Execute.Sql("""
                    DROP TABLE Task;
                    DROP TYPE task_type;
                    DROP TYPE task_state;
                    """);
    }
}