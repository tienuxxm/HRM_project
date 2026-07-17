# Phase Leave Request - Technical Debt: Orphan Pending Approval

## Summary

During Leave Request Detail / Approval UAT, a pending leave request did not show the Official Decision Panel even when the UI layout was close to the approved Stitch design. Further verification showed this is not only a UI issue. The request belongs to an employee/user that has been soft-deleted and no longer has department/position data available for dynamic approver matching.

## Evidence

Runtime request checked:

- Leave Request ID: `204882c1-d032-41f1-bb9a-f847c4b95681`
- Requester code: `WITH091507`
- Requester name: `UAT_Delete_WithHistory_091507`
- Requester employee active: `false`
- Requester user: `with091507`
- Requester user deleted: `true`
- Requester department: `null`
- Requester position: `null`
- Matching approver assignment: none

The approval rule is evaluated dynamically at detail/approval time:

- current user must have `APPROVE_LEAVE_REQUEST`
- current user must map to an active employee
- that employee must have an active `leave_approver_assignment`
- assignment must match the requester's current department/position
- approver cannot approve their own request

Because the requester's department/position are now null, newly granted approver permissions or assignments do not automatically make this old pending request approvable.

## Technical Debt

Pending leave requests can become orphaned when the requester employee/user is soft-deleted or offboarded. The system currently does not snapshot approval routing data on the leave request, and it does not automatically re-route pending requests to HR/Admin after requester offboarding.

This creates ambiguity:

- Who should approve an old pending request after the requester is soft-deleted?
- Should soft-delete block while pending leave requests exist?
- Should pending requests be auto-canceled during offboarding?
- Should department/position/approver route be snapshotted when the request is created?
- Should Admin/HR have a rescue queue for orphan pending requests?

## Proposed Future Options

Option A - Block offboarding when pending leave requests exist:

- Safest for data integrity.
- Requires user/admin to cancel or resolve pending requests before employee deletion.

Option B - Auto-cancel pending requests during offboarding:

- Simple operational behavior.
- Must create audit trail and user-facing reason.

Option C - Snapshot approval route at request creation:

- Keeps old requests approvable even if employee data changes later.
- Requires schema/design change and migration.

Option D - HR/Admin orphan queue:

- Pending requests with missing department/position or deleted requester remain visible to HR/Admin for manual resolution.
- Requires explicit permission and UI state.

## Recommendation

Defer this until the current UI/UX refactor is complete. Then decide a business rule before coding. Preferred direction for enterprise HRM is either:

1. block offboarding while pending requests exist, or
2. auto-cancel with audit trail during offboarding.

Avoid silently reassigning old requests without audit evidence.
