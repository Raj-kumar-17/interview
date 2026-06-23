import React, { useState, useEffect } from "react";
import { useParams } from "react-router-dom";
import {
  addProcedureToPlan,
  addUserToPlanProcedure,
  getPlanProcedures,
  getProcedures,
  getUsers,
  removeAllUsersFromPlanProcedure,
  removeUserFromPlanProcedure,
} from "../../api/api";
import Layout from '../Layout/Layout';
import ProcedureItem from "./ProcedureItem/ProcedureItem";
import PlanProcedureItem from "./PlanProcedureItem/PlanProcedureItem";

const Plan = () => {
  let { id } = useParams();
  const [procedures, setProcedures] = useState([]);
  const [planProcedures, setPlanProcedures] = useState([]);
  const [users, setUsers] = useState([]);

  useEffect(() => {
    (async () => {
      var procedures = await getProcedures();
      var planProcedures = await getPlanProcedures(id);
      var users = await getUsers();

      var userOptions = users.map((u) => ({ label: u.name, value: u.userId }));

      setUsers(userOptions);
      setProcedures(procedures);
      setPlanProcedures(planProcedures);
    })();
  }, [id]);

  const handleAddProcedureToPlan = async (procedure) => {
    const hasProcedureInPlan = planProcedures.some((p) => p.procedureId === procedure.procedureId);
    if (hasProcedureInPlan) return;

    await addProcedureToPlan(id, procedure.procedureId);
    setPlanProcedures((prevState) => {
      return [
        ...prevState,
        {
          planId: id,
          procedureId: procedure.procedureId,
          procedure: {
            procedureId: procedure.procedureId,
            procedureTitle: procedure.procedureTitle,
          },
          planProcedureUsers: [],
        },
      ];
    });
  };

  return (
    <Layout>
      <div className="container pt-4">
        <div className="d-flex justify-content-center">
          <h2>OEC Interview Frontend</h2>
        </div>
        <div className="row mt-4">
          <div className="col">
            <div className="card shadow">
              <h5 className="card-header">Repair Plan</h5>
              <div className="card-body">
                <div className="row">
                  <div className="col">
                    <h4>Procedures</h4>
                    <div>
                      {procedures.map((p) => (
                        <ProcedureItem
                          key={p.procedureId}
                          procedure={p}
                          handleAddProcedureToPlan={handleAddProcedureToPlan}
                          planProcedures={planProcedures}
                        />
                      ))}
                    </div>
                  </div>
                  <div className="col">
                    <h4>Added to Plan</h4>
                    <div>
                      {planProcedures.map((p) => (
                        <PlanProcedureItem
                          key={p.procedure.procedureId}
                          procedure={p}
                          users={users}
                          onAssignUser={async (planId, procedureId, userId) => {
                            await addUserToPlanProcedure(planId, procedureId, userId);
                            setPlanProcedures((prev) =>
                              prev.map((item) =>
                                item.planId === planId && item.procedureId === procedureId
                                  ? {
                                      ...item,
                                      planProcedureUsers: [
                                        ...(item.planProcedureUsers || []),
                                        { user: users.find((u) => u.value === userId) ? { userId, name: users.find((u) => u.value === userId).label } : { userId, name: "" } },
                                      ],
                                    }
                                  : item
                              )
                            );
                          }}
                          onRemoveUser={async (planId, procedureId, userId) => {
                            await removeUserFromPlanProcedure(planId, procedureId, userId);
                            setPlanProcedures((prev) =>
                              prev.map((item) =>
                                item.planId === planId && item.procedureId === procedureId
                                  ? {
                                      ...item,
                                      planProcedureUsers: (item.planProcedureUsers || []).filter(
                                        (pu) => pu.user.userId !== userId
                                      ),
                                    }
                                  : item
                              )
                            );
                          }}
                          onRemoveAllUsers={async (planId, procedureId) => {
                            await removeAllUsersFromPlanProcedure(planId, procedureId);
                            setPlanProcedures((prev) =>
                              prev.map((item) =>
                                item.planId === planId && item.procedureId === procedureId
                                  ? {
                                      ...item,
                                      planProcedureUsers: [],
                                    }
                                  : item
                              )
                            );
                          }}
                        />
                      ))}
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </Layout>
  );
};

export default Plan;
