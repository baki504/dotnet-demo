---
name: review-pr
description: >
  Review a GitHub pull request for code quality and best practices.
  Use this skill whenever the user mentions reviewing a PR, asks to check a pull request,
  pastes a GitHub PR URL, says "review PR #123", "check this PR", "PR review",
  or anything related to evaluating pull request changes.
  Also trigger when the user references a PR number with context suggesting review
  (e.g., "#45 looks ready" or "what do you think of PR 12").
---

# PR Review Skill

Review a GitHub pull request against the project's code quality standards, producing a structured report with scoring.

## How it works

You receive either a PR number, a GitHub PR URL, or a request to review the current branch's PR. You fetch the diff, read the surrounding context in affected files, and evaluate the changes against the project's review checklists.

## Step 1: Identify the PR

Parse the user's input to extract the PR reference:
- **PR number**: `#123` or just `123`
- **GitHub URL**: extract owner/repo and PR number from the URL
- **Current branch**: if no PR specified, check if the current branch has an open PR with `gh pr view`

If ambiguous, ask the user which PR they mean.

## Step 2: Fetch PR information

Use `gh` CLI to gather the PR data:

```bash
# Get PR metadata (title, description, author, base branch)
gh pr view <number> --json title,body,author,baseRefName,headRefName,files,additions,deletions

# Get the full diff
gh pr diff <number>
```

## Step 3: Read surrounding context

For each changed file, read the full file (not just the diff) to understand the context around the changes. This is important because:
- A renamed variable might break other references
- A new method might duplicate existing logic elsewhere in the file
- Import changes might indicate broader architectural shifts

Focus your reading on files with substantive changes. Skip files that are auto-generated, lock files, or configuration with trivial changes.

## Step 4: Determine applicable review checklists

Based on the file types and layers touched, select which review checklists from `.review-prompts/` apply:

| Files changed | Checklist to apply |
|---|---|
| Any `.cs` file | `common.md` (always) |
| Domain/, Application/, Infrastructure/ layers | `backend.md` |
| Controllers/, Views/, ViewModels/ | `frontend.md` |
| Test files (*Tests.cs, *Test.cs) | `test.md` |

Read each applicable checklist file from `.review-prompts/` and evaluate the diff against every item. Only flag items that are actually relevant to the changes — don't report on checklist items that the PR doesn't touch.

## Step 5: Produce the review report

Use the output format defined in `.review-prompts/output-format.md`. Read that file and follow its structure exactly.

The report should include:
- A score based on the scoring rubric
- A concise summary of overall quality
- Specific findings with priority, reason, fix, and impact
- Supplementary notes if relevant

When writing findings:
- Reference specific files and line numbers (e.g., `src/Domain/Employee.cs:42`)
- Quote the problematic code snippet
- Provide a concrete fix, not just "consider improving this"
- Be fair — acknowledge good patterns and decisions, not just problems
- Focus on the changes, not pre-existing issues in untouched code

## Important guidelines

- Review what changed, not the entire codebase. Pre-existing issues in unchanged code are out of scope unless the PR makes them worse.
- If the PR is large (50+ files), summarize by area and focus detailed review on the most critical changes.
- If the diff shows only formatting/whitespace changes, note that and skip detailed review.
- When unsure if something is a real issue or a style preference, lean toward mentioning it at low priority rather than flagging it as high.
