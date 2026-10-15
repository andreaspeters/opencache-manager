# Declaration of AI Usage

Artificial intelligence is a controversial topic. At the same time, it has
become an established part of software development. Many libraries and
dependencies used in my projects, as example those in the Go ecosystem,
are now developed or maintained with the assistance of AI.

Initially, I tried to avoid AI-generated code entirely in my projects. However,
as AI becomes increasingly widespread in software development, maintaining this
separation has become more difficult. Even when I choose not to use AI myself,
AI-generated contributions may still enter my projects through the libraries and
dependencies they rely on.

With this declaration, I want to be transparent about which of my projects
actively use AI during development. **If a repository contains this declaration,
it means that AI has been used in the development of that project.** The extent
of its use is documented on a project-by-project basis in the list below.

## Scope of AI Usage

The specific use of AI varies by project and is limited to the areas listed for
each project:

- Migration from GNUAutoMake to CMake
- Migration from GTK2 to GTK3
- Migration from WebkitGtkSharp-1 to WebkitGtkSharp-3

## Workflow and Quality Assurance

My development process explicitly avoids *vibe coding*. Instead, the AI agent
receives clearly defined, scoped tasks through a Kanban system.

I review and evaluate all AI-generated output. Any changes I disagree with or
that fail to meet my requirements are rejected and, where appropriate, revised.

This approach follows the principles of traditional software engineering: tasks
are planned, work is carried out in a controlled manner, results are reviewed,
and changes are only accepted with my approval.

The purpose of this declaration is to make the use of AI transparent while
making it clear that responsibility for the code and its quality remains with
me.

## AI Models Used

For active AI-assisted development, I exclusively use locally hosted open-weight
models. No proprietary, externally hosted AI models are used for these
development tasks.


