using ServiceManagement.Application.Models.Tasks;
using InvalidCastException = System.InvalidCastException;

namespace ServiceManagement.Presentation.Grpc.Util;

public static class TaskTypeMapper
{
    public static TaskType ToTaskType(Tasks.TaskService.Contracts.TaskType type)
    {
        return type switch
        {
            Tasks.TaskService.Contracts.TaskType.TypeNone => throw new InvalidCastException("TaskType is not defined. Possible server error!"),
            Tasks.TaskService.Contracts.TaskType.TypeTechnicalInspection => TaskType.TechnicalInspection,
            Tasks.TaskService.Contracts.TaskType.TypeRefueling => TaskType.Refueling,
            Tasks.TaskService.Contracts.TaskType.TypeCleaning => TaskType.Cleaning,
            Tasks.TaskService.Contracts.TaskType.TypeRepair => TaskType.Repair,
            Tasks.TaskService.Contracts.TaskType.TypeLoading => TaskType.Loading,
            _ => throw new InvalidCastException($"Unknown TaskTypeProto value: {type}"),
        };
    }

    public static Tasks.TaskService.Contracts.TaskType ToProtoTaskType(TaskType type)
    {
        return type switch
        {
            TaskType.TechnicalInspection => Tasks.TaskService.Contracts.TaskType.TypeTechnicalInspection,
            TaskType.Refueling => Tasks.TaskService.Contracts.TaskType.TypeRefueling,
            TaskType.Cleaning => Tasks.TaskService.Contracts.TaskType.TypeCleaning,
            TaskType.Repair => Tasks.TaskService.Contracts.TaskType.TypeRepair,
            TaskType.Loading => Tasks.TaskService.Contracts.TaskType.TypeLoading,
            _ => throw new InvalidCastException($"Unknown TaskType value: {type}"),
        };
    }
}