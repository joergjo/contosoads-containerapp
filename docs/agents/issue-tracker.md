# Issue tracker: GitHub

Issues and specs live in `joergjo/contosoads-containerapp`. Use the `gh` CLI from the repository root.

## Ticket operations

- Before the first issue creation in a session, confirm `gh repo view --json nameWithOwner` resolves to `joergjo/contosoads-containerapp`; use `--repo joergjo/contosoads-containerapp` if needed.
- Publish a ticket: `gh issue create --title "..." --body-file <file>`.
- Fetch a ticket: `gh issue view <number> --json number,title,body,labels,comments`.
- List tickets: `gh issue list --state open --json number,title,body,labels`; add appropriate label filters.
- Comment: `gh issue comment <number> --body "..."`.
- Apply or remove labels: `gh issue edit <number> --add-label "..."` or `--remove-label "..."`.
- Close: post an explanatory comment, then run `gh issue close <number>`.

## Sub-issues and dependencies

Use native GitHub sub-issues and issue dependencies when available. API calls require numeric database IDs, obtained with `gh api repos/joergjo/contosoads-containerapp/issues/<number> --jq .id`.

- Link a child: `gh api --method POST repos/joergjo/contosoads-containerapp/issues/<parent>/sub_issues -F sub_issue_id=<child-db-id>`.
- Add a blocker: `gh api --method POST repos/joergjo/contosoads-containerapp/issues/<child>/dependencies/blocked_by -F issue_id=<blocker-db-id>`.
- Count open blockers: `gh api repos/joergjo/contosoads-containerapp/issues/<number> --jq .issue_dependencies_summary.blocked_by`.

Where native links are unavailable, put `Part of #<parent>` or `Blocked by: #<number>` at the top of the child body and list children in the parent. A ticket is startable only when every blocker is closed.

## Pull requests as a triage surface

**PRs as a request surface: no.**

## Wayfinding

Use a single issue labelled `wayfinder:map` with Notes, Decisions-so-far, and Fog sections. Link child tickets as sub-issues, with a parent task list as fallback. Label children `wayfinder:research`, `wayfinder:prototype`, `wayfinder:grilling`, or `wayfinder:task`; create missing labels when using this workflow.

Pick the first open, unassigned child in map order with no open blockers. Claim it with `gh issue edit <number> --add-assignee @me` before implementation. Resolve with an explanatory comment, close the ticket, and append the result and link to the map's Decisions-so-far section.
