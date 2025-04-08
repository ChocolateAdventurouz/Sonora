# Contributing to Sonora

First off, thanks for taking the time to read the contribution guide!

See the [Table of Contents](#table-of-contents) for different ways to help. Please make sure to read the relevant section before making your contribution.

> And if you like the project, but don't have time to contribute, that's fine. There are other ways to support the project and show your appreciation:
> - Star the project :star:
> - Link this project in your project's readme
> - Tell others about it

## Table of Contents

- [I Have a Question](#i-have-a-question)
- [I Want To Contribute](#i-want-to-contribute)
- [Reporting Bugs](#reporting-bugs)
- [Suggesting Enhancements and Features](#suggesting-enhancements-and-features)
- [Improving The Documentation](#improving-the-documentation)
- [Styleguides](#styleguides)
- [Commit Messages](#commit-messages)

## I Have a Question

Before asking a question, we assume that you have read the available [Documentation](https://sonora-docs.pages.dev). Also take a look at the [Issues](https://github.com/ImAxel0/Sonora/issues) tab for possible related topics.

If you then still feel the need to ask a question and need clarification, do the following:

- Open a public [Q&A Discussion](https://github.com/ImAxel0/Sonora/discussions/new?category=q-a).
- Provide as much context as you can about what you're running into or what you are trying to achieve.

> The Q&A category is not meant to receive answares from maintainers only. Everyone can answare an unanswered question.

## I Want To Contribute

> ### Legal Notice <!-- omit in toc -->
> When contributing to this project, you must agree that you have authored 100% of the content, that you have the necessary rights to the content and that the content you contribute may be provided under the project licence.

### Reporting Bugs

<!-- omit in toc -->
#### Before Submitting a Bug Report
- Make sure that you are using the latest version.
- Determine if your bug is really a bug and not an error on your side.
- Make sure that you have read the [documentation](https://sonora-docs.pages.dev). If you are looking for support, you might want to check [this section](#i-have-a-question).
- Check if there is not already a bug report existing for your bug or error in the [bug tracker](https://github.com/ImAxel0/Sonora/issues?q=label%3Abug), to see if other users have experienced (and potentially already solved) the same issue you are having.
- Try to to find a way to reproduce the bug.
- Collect all the needed informations which can be helpful to share.

<!-- omit in toc -->
#### How Do I Submit a Bug Report?
Use GitHub issues to track bugs and errors. If you run into an issue with the project:

- Open an [Issue](https://github.com/ImAxel0/Sonora/issues/new).
- Please provide as much context as possible and describe the *reproduction steps* that someone else can follow to recreate the issue on their own. (This usually includes your code example snippet)

### Suggesting Enhancements and Features

<!-- omit in toc -->
#### Before Submitting a Request
- Do not use the [Issues](https://github.com/ImAxel0/Sonora/issues) tab to suggest enhancements and/or new features.
- Read the [documentation](https://sonora-docs.pages.dev) carefully and find out if the functionality is already covered.
- Look if the request has been already  suggested in the [Ideas](https://github.com/ImAxel0/Sonora/discussions/categories/ideas) or [Ideas (Developers)](https://github.com/ImAxel0/Sonora/discussions/categories/ideas-developers) discussion tabs.
- Find out whether your idea fits with the scope of the project. The feature should be generic and not enforce users to follow a strict workflow. If your addition just target a minority of users, consider writing a separate add-on library.

<!-- omit in toc -->
#### How Do I Submit a Suggestion?
Use the [Ideas](https://github.com/ImAxel0/Sonora/discussions/categories/ideas) dicussion tab to suggest an enhancement or new feature.

- Use a **clear and descriptive title** for the discussion to identify the suggestion.
- Provide a **step-by-step description of the suggested enhancement**.
- **Explain why this enhancement would be useful** to most users.

> Depending on the size and scope of it and available time of maintainers, submitting a feature request does not guarantee that it will be implemented.

### Improving The Documentation
If you want to help in writing or enhance documentation for Sonora, take a look at the [Sonora-docs](https://github.com/ImAxel0/Sonora-docs) repository. It is built with [Astro](https://astro.build/) and uses MDX (an extension of markdown) so everyone should be able to understand and write it.

## Submitting Pull Requests

### Before Submitting a Pull Request
- Read the [Styleguides](#styleguides) below.
- Usage of new external libraries or NuGet packages **is prohibited**.
- If the request aims at fixing a bug without changing the framework behaviour, go ahead and send the pull request.
- If the request adds new features or can have an impact on how the framework behave, before starting to write it first open a discussion in the [Ideas (Developers)](https://github.com/ImAxel0/Sonora/discussions/categories/ideas-developers) category to see if it is in the scope of the project. These includes:
	- Creating new classes, interfaces and so on.
	- Changing (NOT enhancing) current implementations.

## Styleguides

### Code
- Do not modify syntax, format and comments of existing code before opening a discussion.
- Do not modify current implementations without discussing yours in [Ideas (Developers)](https://github.com/ImAxel0/Sonora/discussions/categories/ideas-developers).
- Do not use public fields, use properties instead.
- Do not submit large commits (300+ new lines) since it is harder to get an overview of what the commit does. Instead, use small commits with a clear summary/description and make use of code comments if necessary.
- Always add explanatory XML comments using `///` if the feature is meant to be used by end users and to be included in the documentation.
- While not strictly demanded, try to follow C# convential style [Rules](https://google.github.io/styleguide/csharp-style.html).

### Commit Messages
- Start with uppercase letter.
- Use few words to make the commit summary remarkable. If you feel like the commit needs further explanations, use the description tab.

## End Message
Thanks to all the contributors who have and will make the project be better.
