import { ChevronRightIcon } from '@chakra-ui/icons';
import { Box, Flex, IconButton, useBreakpointValue } from '@chakra-ui/react';
import React, { FC, useState } from 'react';
import { Route, Switch } from 'react-router-dom';
import History from '../pages/history/History';
import JobRunOverview from '../pages/job-runs/JobRunOverview';
import JobTaskEditor from '../pages/job-tasks/JobTaskEditor';
import JobAdd from '../pages/jobs/JobAdd';
import JobList from '../pages/jobs/JobList';
import JobOverview from '../pages/jobs/JobOverview';
import ServerAdd from '../pages/servers/ServerAdd';
import ServerList from '../pages/servers/ServerList';
import ServerOverview from '../pages/servers/ServerOverview';
import SettingsOverview from '../pages/settings/SettingsOverview';
import NavLayout from './NavLayout';

const smVariant = { navigation: 'drawer', navigationButton: true };
const mdVariant = { navigation: 'sidebar', navigationButton: false };

const MainLayout: FC = () => {
    const [isSidebarOpen, setSidebarOpen] = useState(false);
    const variants = useBreakpointValue({ base: smVariant, md: mdVariant });
    const toggleSidebar = () => setSidebarOpen(!isSidebarOpen);

    return (
        <Flex height="100%">
            {variants?.navigationButton ? (
                <IconButton
                    icon={<ChevronRightIcon w={8} h={8} />}
                    colorScheme="blackAlpha"
                    variant="outline"
                    onClick={() => toggleSidebar()}
                    position="fixed"
                    top="12px"
                    left="12px"
                    aria-label=""
                    zIndex="100"
                    background="white"
                    _hover={{
                        background: 'white',
                    }}
                />
            ) : null}
            <NavLayout
                variant={variants?.navigation}
                isOpen={isSidebarOpen}
                onClose={toggleSidebar}
            ></NavLayout>

            <Box flex="1" padding="24px">
                <Switch>
                    <Route
                        path="/settings"
                        render={(props) => <SettingsOverview {...props} />}
                    />
                    <Route
                        path="/servers"
                        render={(props) => <ServerList {...props} />}
                    />
                    <Route
                        path="/server/new"
                        render={(props) => <ServerAdd {...props} />}
                    />
                    <Route
                        path="/server/:id"
                        render={(props) => <ServerOverview {...props} />}
                    />
                    <Route
                        path="/jobs"
                        render={(props) => <JobList {...props} />}
                    />
                    <Route
                        path="/job/new"
                        render={(props) => <JobAdd {...props} />}
                    />
                    <Route
                        path="/job/:jobId/task/add"
                        render={(props) => <JobTaskEditor {...props} />}
                    />
                    <Route
                        path="/job/:jobId/task/:id"
                        render={(props) => <JobTaskEditor {...props} />}
                    />
                    <Route
                        path="/job/:id"
                        render={(props) => <JobOverview {...props} />}
                    />
                    <Route
                        path="/run/:id"
                        render={(props) => <JobRunOverview {...props} />}
                    />
                    <Route
                        path="/history"
                        render={(props) => <History {...props} />}
                    />
                </Switch>
            </Box>
        </Flex>
    );
};

export default MainLayout;
