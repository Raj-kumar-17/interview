import React, { useMemo } from "react";
import ReactSelect from "react-select";

const PlanProcedureItem = ({ procedure, users, onAssignUser, onRemoveUser, onRemoveAllUsers }) => {
    const currentValues = useMemo(() => {
        if (!procedure.planProcedureUsers) return [];
        return procedure.planProcedureUsers.map((item) => ({
            label: item.user.name,
            value: item.user.userId,
        }));
    }, [procedure.planProcedureUsers]);

    const handleAssignUserToProcedure = async (selected) => {
        const newValues = selected || [];
        const existingUserIds = currentValues.map((item) => item.value);
        const newUserIds = newValues.map((item) => item.value);

        const addedUserIds = newUserIds.filter((id) => !existingUserIds.includes(id));
        const removedUserIds = existingUserIds.filter((id) => !newUserIds.includes(id));

        if (addedUserIds.length > 0) {
            await Promise.all(
                addedUserIds.map((userId) => onAssignUser(procedure.planId, procedure.procedureId, userId))
            );
        }

        if (removedUserIds.length > 0) {
            await Promise.all(
                removedUserIds.map((userId) => onRemoveUser(procedure.planId, procedure.procedureId, userId))
            );
        }
    };

    return (
        <div className="py-2">
            <div className="d-flex align-items-center justify-content-between">
                <div>{procedure.procedure.procedureTitle}</div>
                <button
                    className="btn btn-sm btn-outline-danger"
                    onClick={() => onRemoveAllUsers(procedure.planId, procedure.procedureId)}
                >
                    Remove All
                </button>
            </div>

            <ReactSelect
                className="mt-2"
                placeholder="Select User to Assign"
                isMulti={true}
                options={users}
                value={currentValues}
                onChange={(e) => handleAssignUserToProcedure(e)}
            />
        </div>
    );
};

export default PlanProcedureItem;
