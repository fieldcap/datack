import { ChevronRightIcon } from '@chakra-ui/icons';
import { Box, Flex, IconButton, useBreakpointValue } from '@chakra-ui/react';
import React, { FC, useState } from 'react';
import { Route, Switch } from 'react-router-dom';
import JobOverview from '../pages/jobs/JobOverview';
import ServerList from '../pages/servers/ServerList';
import ServerOverview from '../pages/servers/ServerOverview';
import StepEditor from '../pages/steps/StepEditor';
import NavLayout from './NavLayout';

const smVariant = { navigation: 'drawer', navigationButton: true };
const mdVariant = { navigation: 'sidebar', navigationButton: false };

const MainLayout: FC = () => {
    const [isSidebarOpen, setSidebarOpen] = useState(false);
    const variants = useBreakpointValue({ base: smVariant, md: mdVariant });
    const toggleSidebar = () => setSidebarOpen(!isSidebarOpen);

    return (
        <Flex>
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
                        path="/servers"
                        render={(props) => <ServerList {...props} />}
                    />
                    <Route
                        path="/server/:id"
                        render={(props) => <ServerOverview {...props} />}
                    />
                    <Route
                        path="/job/:id"
                        render={(props) => <JobOverview {...props} />}
                    />
                    <Route
                        path="/step/:id"
                        render={(props) => <StepEditor {...props} />}
                    />
                </Switch>
            </Box>
        </Flex>
    );
};

export default MainLayout;
