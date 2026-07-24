export function initScrollSpy(container, dotNetRef) {
    const sections = Array.from(container.querySelectorAll('[data-repo-section]'));

    const observer = new IntersectionObserver(entries => {
        const visible = entries.filter(e => e.isIntersecting);
        if (visible.length === 0) {
            return;
        }

        visible.sort((a, b) => a.boundingClientRect.top - b.boundingClientRect.top);
        const repo = visible[0].target.getAttribute('data-repo-section');
        dotNetRef.invokeMethodAsync('OnSectionActivated', repo);
    }, {
        root: container,
        rootMargin: '0px 0px -70% 0px',
        threshold: 0
    });

    sections.forEach(section => observer.observe(section));

    return {
        dispose: () => observer.disconnect()
    };
}

export function scrollToSection(container, repo) {
    const section = container.querySelector(`[data-repo-section="${CSS.escape(repo)}"]`);
    section?.scrollIntoView({ behavior: 'smooth', block: 'start' });
}

export function scrollRepoItemIntoView(container, repo) {
    const item = container.querySelector(`[data-repo-item="${CSS.escape(repo)}"]`);
    item?.scrollIntoView({ block: 'nearest' });
}
