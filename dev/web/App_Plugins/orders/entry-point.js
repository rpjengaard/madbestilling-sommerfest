// [CHANGE: fjerner Umbracos default Welcome-dashboard så vores statistik vises i Content] Related: umbraco-package.json, content-stats-dashboard.js
const ALIASES_TO_REMOVE = [
    'Umb.Dashboard.Welcome',
    'Umb.Dashboard.UmbracoNews',
    'Umb.Dashboard.ContentIntro',
];

const tryRemove = (registry) => {
    ALIASES_TO_REMOVE.forEach(alias => {
        try { registry.unregister?.(alias); } catch {}
        try { registry.exclude?.(alias); } catch {}
    });
};

export const onInit = (host, extensionRegistry) => {
    tryRemove(extensionRegistry);
    setTimeout(() => tryRemove(extensionRegistry), 300);
    setTimeout(() => tryRemove(extensionRegistry), 1500);
};
