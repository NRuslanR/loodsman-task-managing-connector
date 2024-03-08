using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SUPR;
using UMP.Loodsman.Adapters;
using UMP.Loodsman.Dtos.SUPR;
using Task = System.Threading.Tasks.Task;

namespace LoodsmanTaskManagingConnectorObjects.Services
{
    public class StandardLoodsmanTaskService : ILoodsmanTaskService
    {
        private readonly IUsers userList;
        private readonly IWBSSystem wbsSystem;
        private readonly IWbsSystemAdapter wbsSystemAdapter;

        public StandardLoodsmanTaskService(IWbsSystemAdapter wbsSystemAdapter)
        {
            this.wbsSystemAdapter = wbsSystemAdapter;
            wbsSystem = wbsSystemAdapter.WbsSystem;
            userList = wbsSystem.GetUserList();
        }

        public async Task<IEnumerable<ITask>> CreateTasksByDocumentAttachmentAsync(IEnumerable<NewTaskDto> taskInfos,
            long documentId, CancellationToken cancellationToken)
        {
            return await Task.Run(() => CreateTasksByDocumentAttachment(taskInfos, documentId), cancellationToken);
        }

        /// <summary>
        ///     refactor: extract constants in separate classes
        /// </summary>
        /// <param name="taskInfos"></param>
        /// <param name="documentId"></param>
        /// <returns></returns>
        public IEnumerable<ITask> CreateTasksByDocumentAttachment(IEnumerable<NewTaskDto> taskInfos, long documentId)
        {
            wbsSystem.RefreshCache();

            return CreateTasksByDocumentAttachment(taskInfos, documentId, null);
        }

        private IEnumerable<ITask> CreateTasksByDocumentAttachment(IEnumerable<NewTaskDto> taskInfos, long documentId,
            int? parentTaskId)
        {
            var tasks = new List<ITask>();

            foreach (var taskInfo in taskInfos)
            {
                var task = CreateTaskByDocumentAttachment(taskInfo, documentId, parentTaskId);

                foreach (var subTask in CreateTasksByDocumentAttachment(taskInfo.SubTasks, documentId, task.ID))
                    task.AddSubTask(subTask as SUPR.Task);

                task.SaveChanges(0);

                tasks.Add(task);
            }

            return tasks;
        }

        private ITask CreateTaskByDocumentAttachment(NewTaskDto taskInfo, long documentId, int? parentTaskId)
        {
            var task = parentTaskId.HasValue ? wbsSystem.NewSubtask(parentTaskId.Value) : wbsSystem.NewTask();

            task.Topic = taskInfo.Topic;
            task.Author = userList.FindByName(taskInfo.Author.Name);
            task.CalcDirection = GetCalcDirectionById(taskInfo.CalcDirection);
            task.State = GetStateById(taskInfo.State);
            task.Checker = userList.FindByName(taskInfo.Checker.Name);
            task.PlanDateStart = taskInfo.PlanDateStart;
            task.PlanDateFinish = taskInfo.PlanDateFinish;
            task.DateStart = taskInfo.DateStart;
            task.DateFinish = taskInfo.DateFinish;
            task.Deadline = taskInfo.Deadline;
            task.Description = taskInfo.Description;
            task.PlanStartRestriction = GetDeadlineRestrictionById(taskInfo.PlanStartRestriction);
            task.PlanFinishRestriction = GetDeadlineRestrictionById(taskInfo.PlanFinishRestriction);
            task.Priority = taskInfo.Priority;
            task.Worker = userList.FindByName(taskInfo.Worker.Name);
            task.WorkerIsTrusted = taskInfo.WorkerIsTrusted;

            return task;
        }

        private TCalcDirection GetCalcDirectionById(long id)
        {
            return id == 0 ? TCalcDirection.cdAsSoonAsPossible :
                id == 1 ? TCalcDirection.cdAsLateAsPossible : TCalcDirection.cdAsSoonAsPossible;
        }

        private TTaskState GetStateById(long id)
        {
            return id == 0 ? TTaskState.tsNew :
                id == 1 ? TTaskState.tsIssued :
                id == 2 ? TTaskState.tsPerformed :
                id == 3 ? TTaskState.tsPaused :
                id == 4 ? TTaskState.tsCompleted :
                id == 5 ? TTaskState.tsArchive :
                id == 6 ? TTaskState.tsCancelled :
                id == 7 ? TTaskState.tsScheduled :
                id == 8 ? TTaskState.tsReached :
                id == 9 ? TTaskState.tsChecking : throw new ArgumentException($"Task state id = {id} is not correct");
        }

        private TDeadlineRestriction GetDeadlineRestrictionById(long id)
        {
            return id == 1 ? TDeadlineRestriction.drFixed : TDeadlineRestriction.drNone;
        }
    }
}